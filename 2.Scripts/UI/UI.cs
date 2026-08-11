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

    // 승리/게임오버 연출(CTS·트윈)을 모두 정리
    public void StopEndScreenAnimations()
    {
        victoryImageCts?.Cancel();
        victoryImageCts?.Dispose();
        victoryImageCts = null;
        victoryImageTween?.Kill();
        victoryImageTween = null;

        gameOverImageCts?.Cancel();
        gameOverImageCts?.Dispose();
        gameOverImageCts = null;
        gameOverImageTween?.Kill();
        gameOverImageTween = null;
    }

    public async UniTask ShowVictoryUIAsync()
    {
        victoryImageCts = RestartEndScreenCts(victoryImageCts, victoryImageTween);
        victoryImageTween = null;
        await ShowEndScreenAsync(victoryUI.gameObject, victoryUI.victoryImage, victoryImageCts.Token);
    }

    public async UniTask ShowGameOverUIAsync()
    {
        gameOverImageCts = RestartEndScreenCts(gameOverImageCts, gameOverImageTween);
        gameOverImageTween = null;
        await ShowEndScreenAsync(gameOverUI.gameObject, gameOverUI.gameOverImage, gameOverImageCts.Token);
    }

    private CancellationTokenSource RestartEndScreenCts(CancellationTokenSource cts, Tweener tween)
    {
        cts?.Cancel();
        cts?.Dispose();
        tween?.Kill();

        return CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    private async UniTask ShowEndScreenAsync(GameObject screen, Image image, CancellationToken ct)
    {
        SwitchTo(screen);

        await ChangeImageAlphaAsync(image, 1f, 1.5f, null, ct);

        RestartButton.gameObject.SetActive(true);
    }

    public async UniTask ChangeImageAlphaAsync(Image image, float targetAlpha, float duration, System.Action onComplete, CancellationToken ct = default)
    {
        // 일시정지(timeScale = 0) 중에도 UI 페이드는 진행돼야 함
        Tweener tween = image.DOFade(targetAlpha, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
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
        GameEvents.RaisePlaySound(SoundType.Restart);
        StopEndScreenAnimations();
        GameEvents.RaiseGameRestart();
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

        if (instance == this)
        {
            instance = null;
        }
    }
}
