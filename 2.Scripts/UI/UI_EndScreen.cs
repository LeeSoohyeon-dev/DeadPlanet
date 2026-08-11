using UnityEngine;
using UnityEngine.UI;

// 승리/게임오버 화면 공통 베이스 (활성화 시 페이드 인 준비)
public abstract class UI_EndScreen : MonoBehaviour
{
    protected abstract Image ScreenImage { get; }

    private void OnEnable()
    {
        if (ScreenImage == null)
            return;

        Color color = ScreenImage.color;
        color.a = 0f;
        ScreenImage.color = color;
    }
}
