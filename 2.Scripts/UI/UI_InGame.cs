using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Febucci.UI;


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
        lootButtonImage.color = isActive ? new Color32(225, 228, 0, 255) : new Color32(206, 206, 206, 255); 
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
        healthBar.value = (float)currentHealth / maxHealth;
    }

    public void UpdateMissionUI(int remainingEnemy, int killedEnemy)
    {
        remainingEnemyText.text = remainingEnemy.ToString();
        killedEnemyText.text = killedEnemy.ToString();
    }
}
