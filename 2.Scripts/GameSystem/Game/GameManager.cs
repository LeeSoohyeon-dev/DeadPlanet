
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    public bool isFriendlyFire;
    
    private bool hasProcessedStageScene = false;

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
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        GameEvents.OnGameVictory += Victory;
        GameEvents.OnGameOver += GameOver;
        GameEvents.OnGameRestart += RestartGame;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameEvents.OnGameVictory -= Victory;
        GameEvents.OnGameOver -= GameOver;
        GameEvents.OnGameRestart -= RestartGame;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1 && !hasProcessedStageScene)
        {
            hasProcessedStageScene = true;
            UI.instance.SwitchTo(UI.instance.inGameUI.gameObject);
            GameEvents.OnPlaySound?.Invoke(SoundType.BGM);
            UI.instance.StartCoroutine(UI.instance.ChangeImageAlpha(UI.instance.fadeImage, 0, 1.5f, UI.instance.StartTheGame));
        }
        else if (scene.buildIndex == 0)
        {
            hasProcessedStageScene = false;
        }
        else if (scene.buildIndex == 1 && hasProcessedStageScene)
        {
            GameEvents.OnPlaySound?.Invoke(SoundType.BGM);
        }
    }

    public void Victory()
    {
        Debug.Log("Victory");

        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {   
        GameEvents.OnStopBGM?.Invoke();
        Coroutine uiCoroutine = UI.instance.ShowVictoryUICo();
        GameEvents.OnSlowMotion?.Invoke(1.5f);
        GameEvents.OnStopAllAudio?.Invoke();
        GameEvents.OnStopBGM?.Invoke();
        GameEvents.OnPlaySound?.Invoke(SoundType.Victory);
        
        yield return uiCoroutine;
        GameEvents.OnPauseTime?.Invoke();
    }

    public void GameOver()
    {

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {   
        GameEvents.OnStopBGM?.Invoke();
        Coroutine uiCoroutine = UI.instance.ShowGameOverUICo();
        GameEvents.OnSlowMotion?.Invoke(1.5f);
        GameEvents.OnStopAllAudio?.Invoke();
        GameEvents.OnPlaySound?.Invoke(SoundType.GameOver);
        
        yield return uiCoroutine;
        GameEvents.OnPauseTime?.Invoke();
    }
    
    public void RestartGame()
    {
        Debug.Log("=== RestartGame 메서드가 호출되었습니다! ===");
        
        hasProcessedStageScene = true;
        
        GameEvents.OnResumeTime?.Invoke();
        
        StartCoroutine(RestartGameSequence());
    }
    
    private IEnumerator RestartGameSequence()
    {
        UI.instance.RestartButton.gameObject.SetActive(false);
        
        yield return UI.instance.StartCoroutine(UI.instance.ChangeImageAlpha(UI.instance.fadeImage, 1, 1f, null));
        
        UI.instance.SwitchTo(UI.instance.inGameUI.gameObject);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        yield return UI.instance.StartCoroutine(UI.instance.ChangeImageAlpha(UI.instance.fadeImage, 0, 1f, null));
    }

}
