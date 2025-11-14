using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Febucci.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections;
using DG.Tweening;

public class UI_Start : MonoBehaviour
{
    [SerializeField] private TypewriterByCharacter touchText;
    [SerializeField] private Button touchArea;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;

    [SerializeField] private float progressCompleteThreshold = 0.95f;
    [SerializeField] private float sliderAnimationDuration = 0.3f;

    private bool isSceneLoaded = false;
    private bool isTouchEnabled = false;
    private AsyncOperation preloadedScene;
    private bool isLoading = false;
    private Tweener sliderTween;
    private float currentTargetProgress = 0f;

    private void Awake()
    {
        InitializeUI();
        SetupTouchArea();
        InitializeProgressSlider();
    }

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private void Update()
    {
        UpdateLoadingProgress();
        HandleDirectInput();
    }

    private void InitializeUI()
    {
        if (touchText != null)
            touchText.gameObject.SetActive(false);
    }

    private void SetupTouchArea()
    {
        if (touchArea != null)
        {
            touchArea.onClick.AddListener(OnTouchAreaClicked);
            touchArea.interactable = false;
        }
    }

    private void InitializeProgressSlider()
    {
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
        }
    }

    private void ShowTouchPrompt()
    {
        isTouchEnabled = true;

        if (touchArea != null)
        {
            touchArea.gameObject.SetActive(true);
            touchArea.interactable = true;
        }

        if (touchText != null)
        {
            touchText.gameObject.SetActive(true);
            touchText.StartShowingText();
        }
    }

    private void UpdateLoadingProgress()
    {
        if (!isLoading || preloadedScene == null || progressSlider == null)
            return;

        float rawProgress = preloadedScene.progress;
        float targetProgress = Mathf.Clamp01(rawProgress / 0.9f);

        if (sliderTween == null || !sliderTween.IsActive() || Mathf.Abs(targetProgress - currentTargetProgress) > 0.01f)
        {
            sliderTween?.Kill();
            sliderTween = progressSlider.DOValue(targetProgress, sliderAnimationDuration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(false);
            currentTargetProgress = targetProgress;
        }

        float currentProgress = progressSlider.value;
        progressText.text = $"{Mathf.Round(currentProgress * 100)}%";

        if (targetProgress >= 1f && currentProgress >= progressCompleteThreshold)
        {
            OnLoadingComplete();
        }
    }

    private void HandleDirectInput()
    {
        if (!isTouchEnabled || !isSceneLoaded)
            return;

        bool inputDetected = false;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            inputDetected = true;

        if (Input.GetMouseButtonDown(0))
            inputDetected = true;

        if (inputDetected)
            OnTouchAreaClicked();
    }

    private IEnumerator LoadSceneAsync()
    {
        isLoading = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scavenger");
        asyncLoad.allowSceneActivation = false;
        preloadedScene = asyncLoad;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        OnSceneLoadComplete();
    }

    private void OnSceneLoadComplete()
    {
        isSceneLoaded = true;

    }

    private void OnLoadingComplete()
    {
        sliderTween?.Kill();
        sliderTween = progressSlider.DOValue(1f, sliderAnimationDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                progressText.text = "100%";
            });

        isLoading = false;
        ShowTouchPrompt();
    }

    private void OnTouchAreaClicked()
    {
        if (!isTouchEnabled || !isSceneLoaded)
            return;

        GameEvents.OnPlaySound?.Invoke(SoundType.Click);
        DisableTouchArea();
        HideUIElements();

        StartSceneTransition();
    }

    private void DisableTouchArea()
    {
        isTouchEnabled = false;
        if (touchArea != null)
            touchArea.interactable = false;
    }

    private void HideUIElements()
    {
        if (touchText != null)
            touchText.gameObject.SetActive(false);

        if (progressSlider != null)
            progressSlider.gameObject.SetActive(false);

        if (progressText != null)
            progressText.gameObject.SetActive(false);
    }

    private void StartSceneTransition()
    {
        StartSceneTransitionAsync().Forget();
    }

    private async UniTask StartSceneTransitionAsync()
    {
        if (preloadedScene != null && UI.instance != null)
        {
            try
            {
                await UI.instance.ChangeImageAlphaAsync(UI.instance.fadeImage, 1, 1f, () => {
                    preloadedScene.allowSceneActivation = true;
                }, this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                Debug.Log("씬 전환 시퀀스가 취소되었습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"씬 전환 시퀀스 실행 중 오류 발생: {e.Message}");
            }
        }
    }

    public bool IsTouchEnabled => isTouchEnabled && isSceneLoaded;

    private void OnDestroy()
    {
        sliderTween?.Kill();
    }
}
