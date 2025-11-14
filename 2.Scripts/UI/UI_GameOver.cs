using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour
{
   public Image gameOverImage;

    private void OnEnable()
    {
        gameOverImage.color = new Color(0,0,0,0);
    }

}
