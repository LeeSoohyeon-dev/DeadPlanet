using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using DG.Tweening;


public class UI : MonoBehaviour
{
    public static UI instance;
    public UI_InGame inGameUI { get; private set; }
    public UI_GameOver gameOverUI { get; private set; }
    public UI_Start startUI { get; private set; }
    public UI_Victoty victoryUI { get; private set; }

    [SerializeField] private GameObject[] UIElements;
    [SerializeField] private Button restartButton;
    public Button RestartButton => restartButton;
    public Image fadeImage;
    private CancellationTokenSource victoryImageCts;
    private CancellationTokenSource gameOverImageCts;
    private Tweener fadeImageTween;
    private Tweener victoryImageTween;
    private Tweener gameOverImageTween;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        inGameUI = GetComponentInChildren<UI_InGame>(true);
        gameOverUI = GetComponentInChildren<UI_GameOver>(true);
        startUI = GetComponentInChildren<UI_Start>(true);
        victoryUI = GetComponentInChildren<UI_Victoty>(true);
        fadeImage.gameObject.SetActive(true);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    public void SwitchTo(GameObject uiToSwitchOn)
    {
        foreach (GameObject go in UIElements)
        {
            go.SetActive(false);
        }

        uiToSwitchOn.SetActive(true);
    }

    public async UniTask StartTheGameAsync()
    {
        await ChangeImageAlphaAsync(fadeImage, 0, 1f, null, this.GetCancellationTokenOnDestroy());
    }

    public void StopGameOverImageCoroutine()
    {
        gameOverImageCts?.Cancel();
        gameOverImageCts?.Dispose();
        gameOverImageCts = null;
        gameOverImageTween?.Kill();
    }

    public async UniTask ShowVictoryUIAsync(string message = "VICTORY!")
    {
        SwitchTo(victoryUI.gameObject);
        victoryImageCts?.Cancel();
        victoryImageCts?.Dispose();
        victoryImageTween?.Kill();
        victoryImageCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        await ChangeImageAlphaAsync(victoryUI.victoryImage, 1f, 1.5f, () => {RestartButton.gameObject.SetActive(true);}, victoryImageCts.Token);
    }

    public async UniTask ShowGameOverUIAsync(string message = "GAME OVER!")
    {
        SwitchTo(gameOverUI.gameObject);
        gameOverImageCts?.Cancel();
        gameOverImageCts?.Dispose();
        gameOverImageTween?.Kill();
        gameOverImageCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        await ChangeImageAlphaAsync(gameOverUI.gameOverImage, 1f, 1.5f, () => {RestartButton.gameObject.SetActive(true);}, gameOverImageCts.Token);
    }

    public async UniTask ChangeImageAlphaAsync(Image image, float targetAlpha, float duration, System.Action onComplete, CancellationToken ct = default)
    {
        Tweener tween = image.DOFade(targetAlpha, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                onComplete?.Invoke();
            });

        if (image == fadeImage)
            fadeImageTween = tween;
        else if (image == victoryUI?.victoryImage)
            victoryImageTween = tween;
        else if (image == gameOverUI?.gameOverImage)
            gameOverImageTween = tween;

        try
        {
            await UniTask.WaitUntil(() => !tween.IsActive(), cancellationToken: ct);
        }
        catch (OperationCanceledException)
        {
            tween?.Kill();
            throw;
        }
    }

    private void OnRestartButtonClicked()
    {
        GameEvents.OnPlaySound?.Invoke(SoundType.Restart);
        StopGameOverImageCoroutine();
        GameEvents.OnGameRestart?.Invoke();
    }

    private void OnDestroy()
    {
        victoryImageCts?.Cancel();
        victoryImageCts?.Dispose();
        gameOverImageCts?.Cancel();
        gameOverImageCts?.Dispose();

        fadeImageTween?.Kill();
        victoryImageTween?.Kill();
        gameOverImageTween?.Kill();
    }
}
