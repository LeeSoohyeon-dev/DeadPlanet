using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    public bool isFriendlyFire;

    private const int START_SCENE_INDEX = 0;
    private const int STAGE_SCENE_INDEX = 1;

    private bool hasProcessedStageScene = false;
    private bool isProcessingSequence = false;

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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnGameVictory += Victory;
        GameEvents.OnGameOver += GameOver;
        GameEvents.OnGameRestart += RestartGame;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnGameVictory -= Victory;
        GameEvents.OnGameOver -= GameOver;
        GameEvents.OnGameRestart -= RestartGame;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int sceneIndex = scene.buildIndex;

        if (sceneIndex == START_SCENE_INDEX)
        {
            hasProcessedStageScene = false;
            return;
        }

        if (sceneIndex == STAGE_SCENE_INDEX)
        {
            if (!hasProcessedStageScene)
            {
                hasProcessedStageScene = true;
                UI.instance.SwitchTo(UI.instance.inGameUI.gameObject);
                UI.instance.ChangeImageAlphaAsync(UI.instance.fadeImage, 0, 1.5f, () => UI.instance.StartTheGameAsync().Forget(), this.GetCancellationTokenOnDestroy()).Forget();
            }

            GameEvents.OnPlaySound?.Invoke(SoundType.BGM);
        }
    }

    public void Victory()
    {
        VictorySequenceAsync().Forget();
    }

    private async UniTask VictorySequenceAsync()
    {
        await ExecuteGameEndSequenceAsync(
            () => UI.instance.ShowVictoryUIAsync(),
            SoundType.Victory,
            "Victory"
        );
    }

    public void GameOver()
    {
        GameOverSequenceAsync().Forget();
    }

    private async UniTask GameOverSequenceAsync()
    {
        await ExecuteGameEndSequenceAsync(
            () => UI.instance.ShowGameOverUIAsync(),
            SoundType.GameOver,
            "GameOver"
        );
    }

    public void RestartGame()
    {
        RestartGameSequenceAsync().Forget();
    }

    private async UniTask RestartGameSequenceAsync()
    {
        if (isProcessingSequence)
            return;

        isProcessingSequence = true;
        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        try
        {
            hasProcessedStageScene = true;
            GameEvents.OnResumeTime?.Invoke();
            
            UI.instance.RestartButton.gameObject.SetActive(false);

            await UI.instance.ChangeImageAlphaAsync(UI.instance.fadeImage, 1, 1f, null, ct);

            UI.instance.SwitchTo(UI.instance.inGameUI.gameObject);

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(currentSceneIndex);
            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                // UniTask.Yield == yield return null
                await UniTask.Yield(ct);
            }

            loadOperation.allowSceneActivation = true;
            await UniTask.WaitUntil(() => loadOperation.isDone, cancellationToken: ct);

            await UI.instance.ChangeImageAlphaAsync(UI.instance.fadeImage, 0, 1f, null, ct);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("게임 재시작 시퀀스가 취소되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 재시작 시퀀스 실행 중 오류 발생: {e.Message}");
        }
        finally
        {
            isProcessingSequence = false;
        }
    }

    private async UniTask ExecuteGameEndSequenceAsync(Func<UniTask> showUITask, SoundType soundType, string sequenceName)
    {
        if (isProcessingSequence)
            return;

        isProcessingSequence = true;
        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        try
        {
            UniTask uiTask = showUITask();
            GameEvents.OnSlowMotion?.Invoke(1.5f);
            GameEvents.OnPlaySound?.Invoke(soundType);

            await uiTask;
            GameEvents.OnPauseTime?.Invoke();
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"{sequenceName} 시퀀스가 취소되었습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{sequenceName} 시퀀스 실행 중 오류 발생: {e.Message}");
        }
        finally
        {
            isProcessingSequence = false;
        }
    }

}
