using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Febucci.UI;
using System.Collections;

public class UI_Start : MonoBehaviour
{
    [SerializeField] private TypewriterByCharacter touchText;
    [SerializeField] private Button touchArea;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;

    [SerializeField] private float progressSmoothSpeed = 2f;
    [SerializeField] private float progressCompleteThreshold = 0.95f;

    private bool isSceneLoaded = false;
    private bool isTouchEnabled = false;
    private AsyncOperation preloadedScene;
    private bool isLoading = false;
    private float smoothProgress = 0f;

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
            smoothProgress = 0f;
        }
    }

    private void UpdateLoadingProgress()
    {
        if (!isLoading || preloadedScene == null || progressSlider == null)
            return;

        float rawProgress = preloadedScene.progress;
        float targetProgress = Mathf.Clamp01(rawProgress / 0.9f);
        
        smoothProgress = Mathf.Lerp(smoothProgress, targetProgress, Time.deltaTime * progressSmoothSpeed);
        progressSlider.value = smoothProgress;
        progressText.text = $"{Mathf.Round(smoothProgress * 100)}%";
        
        if (targetProgress >= 1f && smoothProgress >= progressCompleteThreshold)
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
        smoothProgress = 1f;
        progressSlider.value = 1f;
        progressText.text = "100%";
        isTouchEnabled = true;
        isLoading = false;
        
        touchArea.gameObject.SetActive(true);
        touchArea.interactable = true;

        if (touchText != null)
        {
            touchText.gameObject.SetActive(true);
            touchText.StartShowingText();
        }
    }

    private void OnTouchAreaClicked()
    {
        if (!isTouchEnabled || !isSceneLoaded)
            return;

        AudioManager.instance?.PlayAudio(SoundType.Click);
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
        if (preloadedScene != null && UI.instance != null)
        {
            StartCoroutine(UI.instance.ChangeImageAlpha(UI.instance.fadeImage, 1, 1f, () => {
                preloadedScene.allowSceneActivation = true;
            }));
        }
    }

    public bool IsTouchEnabled => isTouchEnabled && isSceneLoaded;
}

