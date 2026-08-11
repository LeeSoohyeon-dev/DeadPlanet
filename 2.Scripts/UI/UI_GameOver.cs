using UnityEngine.UI;

public class UI_GameOver : UI_EndScreen
{
    public Image gameOverImage;

    protected override Image ScreenImage => gameOverImage;
}
