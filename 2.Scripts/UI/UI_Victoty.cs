using UnityEngine;
using UnityEngine.UI;
public class UI_Victoty : MonoBehaviour
{
    public Image victoryImage;

    private void OnEnable()
    {
        victoryImage.color = new Color(0,0,0,0);
    }
}
