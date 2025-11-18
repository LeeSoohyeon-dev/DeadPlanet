using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    [SerializeField] private UI_WeaponSlot[] weaponSlots_UI;
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
    private Tweener remainingEnemyCountTween;
    private Tweener killedEnemyCountTween;
    private Tweener killedEnemyTextScaleTween;
    private float currentRemainingEnemyCount = 0f;
    private float currentKilledEnemyCount = 0f;
    private bool isRemainingEnemyCountInitialized = false;
    private bool isKilledEnemyCountInitialized = false;
    private readonly Color32 activeLootButtonColor = new Color32(255, 235, 59, 255);
    private readonly Color32 inactiveLootButtonColor = new Color32(206, 206, 206, 255);

    private void Awake()
    {
        weaponSlots_UI = GetComponentsInChildren<UI_WeaponSlot>();
    }

    private void OnEnable()
    {
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
                bool isActiveWeapon = weaponSlots[i] == currentWeapon ? true : false;
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
        float targetValue = (float)currentHealth / maxHealth;

        healthBarTween?.Kill();
        healthBarTween = healthBar.DOValue(targetValue, healthBarAnimationDuration)
            .SetEase(Ease.OutQuad);
    }

    public void UpdateMissionUI(int remainingEnemy, int killedEnemy)
    {
        UpdateRemainingEnemyCount(remainingEnemy);
        UpdateKilledEnemyCount(killedEnemy);
    }

    private void UpdateRemainingEnemyCount(int targetValue)
    {
        if (remainingEnemyText == null)
            return;

        remainingEnemyCountTween?.Kill();

        if (!isRemainingEnemyCountInitialized)
        {
            currentRemainingEnemyCount = targetValue;
            remainingEnemyText.text = targetValue.ToString();
            isRemainingEnemyCountInitialized = true;
            return;
        }

        float startValue = currentRemainingEnemyCount;
        int targetInt = targetValue;

        remainingEnemyCountTween = DOTween.To(
            () => startValue,
            x => {
                startValue = x;
                currentRemainingEnemyCount = x;
                remainingEnemyText.text = Mathf.RoundToInt(startValue).ToString();
            },
            targetInt,
            remainingEnemyCountDuration
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() => {
            currentRemainingEnemyCount = targetInt;
            remainingEnemyText.text = targetInt.ToString();
        });
    }

    private void UpdateKilledEnemyCount(int targetValue)
    {
        if (killedEnemyText == null)
            return;

        killedEnemyCountTween?.Kill();

        if (!isKilledEnemyCountInitialized)
        {
            currentKilledEnemyCount = targetValue;
            killedEnemyText.text = targetValue.ToString();
            isKilledEnemyCountInitialized = true;
            return;
        }

        float startValue = currentKilledEnemyCount;
        int targetInt = targetValue;

        killedEnemyCountTween = DOTween.To(
            () => startValue,
            x => {
                startValue = x;
                currentKilledEnemyCount = x;
                killedEnemyText.text = Mathf.RoundToInt(startValue).ToString();
            },
            targetInt,
            killedEnemyCountDuration
        )
        .SetEase(Ease.OutCubic)
        .OnComplete(() => {
            currentKilledEnemyCount = targetInt;
            killedEnemyText.text = targetInt.ToString();
        });

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
        remainingEnemyCountTween?.Kill();
        killedEnemyCountTween?.Kill();
        killedEnemyTextScaleTween?.Kill();
    }
}
