using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;
using DG.Tweening;

public class UI_InGame : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthBar;

    [Header("Mission")]
    [SerializeField] private TypewriterByCharacter missionText;
    [SerializeField] private TextMeshProUGUI remainingEnemyText;
    [SerializeField] private TextMeshProUGUI killedEnemyText;

    [Header("Weapons")]
    private UI_WeaponSlot[] weaponSlots_UI;
    [SerializeField] private Button switchWeaponButton1;
    [SerializeField] private Button switchWeaponButton2;
    [SerializeField] private Button shootButton;
    [SerializeField] private Button reloadButton;
    [SerializeField] private Button lootButton;
    [SerializeField] private Image lootButtonImage;

    [Header("Animation Settings")]
    [SerializeField] private float healthBarAnimationDuration = 0.3f;
    [SerializeField] private float lootButtonColorDuration = 0.2f;
    [SerializeField] private float lootButtonScaleDuration = 0.2f;
    [SerializeField] private float lootButtonActiveScale = 1.15f;
    [SerializeField] private float killedEnemyCountDuration = 0.3f;
    [SerializeField] private float remainingEnemyCountDuration = 0.4f;

    private Tweener healthBarTween;
    private Tweener lootButtonColorTween;
    private Tweener lootButtonScaleTween;
    private Tweener killedEnemyTextScaleTween;
    private CountUpCounter remainingEnemyCounter;
    private CountUpCounter killedEnemyCounter;
    private readonly Color32 activeLootButtonColor = new Color32(255, 235, 59, 255);
    private readonly Color32 inactiveLootButtonColor = new Color32(206, 206, 206, 255);

    // 숫자 카운트업 연출 공통 처리 (첫 갱신은 애니메이션 없이 즉시 표시)
    private sealed class CountUpCounter
    {
        private readonly TextMeshProUGUI text;
        private float current;
        private bool initialized;
        private Tweener tween;

        public CountUpCounter(TextMeshProUGUI text)
        {
            this.text = text;
        }

        public void Reset()
        {
            tween?.Kill();
            initialized = false;
            current = 0f;
        }

        public void Kill() => tween?.Kill();

        // 반환값: 카운트업 애니메이션이 재생됐는지 (첫 초기화면 false)
        public bool SetValue(int target, float duration, Ease ease)
        {
            if (text == null)
                return false;

            tween?.Kill();

            if (!initialized)
            {
                initialized = true;
                current = target;
                text.text = target.ToString();
                return false;
            }

            tween = DOTween.To(
                () => current,
                x =>
                {
                    current = x;
                    text.text = Mathf.RoundToInt(x).ToString();
                },
                target,
                duration
            )
            .SetEase(ease)
            .OnComplete(() =>
            {
                current = target;
                text.text = target.ToString();
            });

            return true;
        }
    }

    private void Awake()
    {
        weaponSlots_UI = GetComponentsInChildren<UI_WeaponSlot>(true);

        remainingEnemyCounter = new CountUpCounter(remainingEnemyText);
        killedEnemyCounter = new CountUpCounter(killedEnemyText);
    }

    private void OnEnable()
    {
        // HUD가 다시 켜질 때(게임 시작/재시작)마다 카운터를 새 판 기준으로 초기화
        // (재시작 이벤트 시점에는 이 오브젝트가 비활성이라 이벤트 구독으로는 받을 수 없음)
        ResetCounters();

        if (missionText != null)
            missionText.StartShowingText(restart: true);

        GameEvents.OnPlayerHealthChanged += UpdateHealthUI;
        GameEvents.OnWeaponUIUpdate += UpdateWeaponUI;
        GameEvents.OnMissionUIUpdate += UpdateMissionUI;
        GameEvents.OnLootButtonUpdate += UpdateLootButton;
   }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= UpdateHealthUI;
        GameEvents.OnWeaponUIUpdate -= UpdateWeaponUI;
        GameEvents.OnMissionUIUpdate -= UpdateMissionUI;
        GameEvents.OnLootButtonUpdate -= UpdateLootButton;
    }

    private void ResetCounters()
    {
        remainingEnemyCounter.Reset();
        killedEnemyCounter.Reset();
    }

    public Button SwitchWeaponButton1 => switchWeaponButton1;
    public Button SwitchWeaponButton2 => switchWeaponButton2;
    public Button ShootButton => shootButton;
    public Button ReloadButton => reloadButton;
    public Button LootButton => lootButton;

    public void UpdateLootButton(bool isActive)
    {
        if (lootButton == null || lootButtonImage == null)
            return;

        Color32 targetColor = isActive ? activeLootButtonColor : inactiveLootButtonColor;
        Vector3 targetScale = isActive ? Vector3.one * lootButtonActiveScale : Vector3.one;

        lootButtonColorTween?.Kill();
        lootButtonColorTween = lootButtonImage.DOColor(targetColor, lootButtonColorDuration)
            .SetEase(Ease.OutQuad);

        lootButtonScaleTween?.Kill();
        lootButtonScaleTween = lootButton.transform.DOScale(targetScale, lootButtonScaleDuration)
            .SetEase(Ease.OutQuad);
    }

    public void UpdateWeaponUI(List<Weapon> weaponSlots, Weapon currentWeapon)
    {
        for (int i = 0; i < weaponSlots_UI.Length; i++)
        {
            if (i < weaponSlots.Count)
            {
                bool isActiveWeapon = weaponSlots[i] == currentWeapon;
                weaponSlots_UI[i].UpdateWeaponSlot(weaponSlots[i], isActiveWeapon);
            }
            else
            {
                weaponSlots_UI[i].UpdateWeaponSlot(null, false);
            }
        }
    }
    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        float targetValue = currentHealth / maxHealth;

        healthBarTween?.Kill();
        healthBarTween = healthBar.DOValue(targetValue, healthBarAnimationDuration)
            .SetEase(Ease.OutQuad);
    }

    public void UpdateMissionUI(int remainingEnemy, int killedEnemy)
    {
        remainingEnemyCounter.SetValue(remainingEnemy, remainingEnemyCountDuration, Ease.OutQuad);

        if (killedEnemyCounter.SetValue(killedEnemy, killedEnemyCountDuration, Ease.OutCubic))
            PlayKilledCountPunch();
    }

    private void PlayKilledCountPunch()
    {
        killedEnemyTextScaleTween?.Kill();
        killedEnemyText.transform.localScale = Vector3.one;

        killedEnemyTextScaleTween = killedEnemyText.transform.DOPunchScale(Vector3.one, 0.5f, 5, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        healthBarTween?.Kill();
        lootButtonColorTween?.Kill();
        lootButtonScaleTween?.Kill();
        killedEnemyTextScaleTween?.Kill();
        remainingEnemyCounter?.Kill();
        killedEnemyCounter?.Kill();
    }
}
