using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    private Coroutine victoryImageCoroutine;
    private Coroutine gameOverImageCoroutine;

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

    public void StartTheGame()
    {
        StartCoroutine(ChangeImageAlpha(fadeImage, 0, 1f, null));
    }
    
    public void StopGameOverImageCoroutine()
    {
        if (gameOverImageCoroutine != null)
        {
            StopCoroutine(gameOverImageCoroutine);
            gameOverImageCoroutine = null;
        }
    }

    public Coroutine ShowVictoryUICo(string message = "VICTORY!")
    {
        SwitchTo(victoryUI.gameObject);
        victoryImageCoroutine = StartCoroutine(ChangeImageAlpha(victoryUI.victoryImage, 1f, 1.5f, () => {RestartButton.gameObject.SetActive(true);}));
        return victoryImageCoroutine;
    }

    public Coroutine ShowGameOverUICo(string message = "GAME OVER!")
    {
        SwitchTo(gameOverUI.gameObject);
        gameOverImageCoroutine = StartCoroutine(ChangeImageAlpha(gameOverUI.gameOverImage, 1f, 1.5f, () => {RestartButton.gameObject.SetActive(true);}));
        return gameOverImageCoroutine;
    }

    public IEnumerator ChangeImageAlpha(Image image, float targetAlpha, float duration, System.Action onComplete)
    {
        
        float time = 0;
        Color currentColor = image.color;
        float startAlpha = currentColor.a;

        while(time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            image.color = new Color(currentColor.r,currentColor.g, currentColor.b,alpha);
            yield return null;
        }

        image.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

        onComplete?.Invoke();
    }

    private void OnRestartButtonClicked()
    { 
        GameEvents.OnPlaySound?.Invoke(SoundType.Restart);
        StopGameOverImageCoroutine();
        GameEvents.OnGameRestart?.Invoke();
    }
}
