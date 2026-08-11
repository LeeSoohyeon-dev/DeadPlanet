using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private Weapon_Data defaultWeaponData;

    // 런타임 전용 (Weapon은 인스펙터 직렬화로 만들 수 없음)
    private Weapon currentWeapon;

    private Player player;
    private PlayerWeaponSlot weaponSlot;
    private PlayerCombat combat;

    private UnityAction switchWeapon1Action;
    private UnityAction switchWeapon2Action;
    private UnityAction shootAction;
    private UnityAction reloadAction;

    private void Awake()
    {
        player = GetComponent<Player>();
        weaponSlot = GetComponent<PlayerWeaponSlot>();
        combat = GetComponent<PlayerCombat>();
    }

    private void Start()
    {
        AssignUIButtonEvents();
        Invoke(nameof(EquipStartingWeapon), 0.1f);
    }

    public void UpdateWeaponUI()
    {
        GameEvents.RaiseWeaponUIUpdate(weaponSlot.GetWeaponSlots(), currentWeapon);
    }

    private void EquipStartingWeapon()
    {
        weaponSlot.InitializeSlots(defaultWeaponData);
        EquipWeapon(0);
    }

    public void SetWeaponReady(bool ready) => combat.SetWeaponReady(ready);
    public bool IsWeaponReady() => combat.IsWeaponReady();
    public bool HasOnlyOneWeapon() => weaponSlot.HasOnlyOneWeapon();

    public Weapon WeaponInSlots(WeaponType weaponType) => weaponSlot.WeaponInSlots(weaponType);
    public List<Weapon> GetWeaponSlots() => weaponSlot != null ? weaponSlot.GetWeaponSlots() : null;
    private void EquipWeapon(int i)
    {
        List<Weapon> slots = weaponSlot.GetWeaponSlots();
        if (i < 0 || i >= slots.Count)
        {
            return;
        }

        // 이미 장착 중인 무기를 다시 장착하면 준비 상태만 풀리므로 무시
        if (slots[i] == currentWeapon)
        {
            return;
        }

        SetWeaponReady(false);
        currentWeapon = slots[i];
        player.weaponVisuals.PlayWeaponEquipAnimation();

        GameEvents.RaisePlaySound(SoundType.WeaponSwitch);

        UpdateWeaponUI();
    }

    public void PickupWeapon(Weapon newWeapon)
    {
        if (weaponSlot.TryMergeAmmo(newWeapon))
        {
            UpdateWeaponUI();
            return;
        }

        if (weaponSlot.IsFull)
        {
            List<Weapon> slots = weaponSlot.GetWeaponSlots();

            int weaponIndex = slots.IndexOf(currentWeapon);
            if (weaponIndex < 0)
                weaponIndex = 0;

            // 실제 슬롯 점유자를 떨어뜨려야 무기가 소실되지 않음 (정상 경로에선 currentWeapon과 동일)
            weaponSlot.ReplaceWeaponAtSlot(weaponIndex, newWeapon, slots[weaponIndex]);
            EquipWeapon(weaponIndex);
            return;
        }

        weaponSlot.AddWeapon(newWeapon);
        UpdateWeaponUI();
    }

    public Weapon CurrentWeapon() => currentWeapon;

    public Transform GunPoint()
    {
        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();
        return weaponModel != null ? weaponModel.bulletSpawnPoint : null;
    }

    private void AssignUIButtonEvents()
    {

        if (UI.instance?.inGameUI != null)
        {
            SetupWeaponButtons();
        }
        else
        {
            Invoke(nameof(AssignUIButtonEvents), 0.1f);
        }
    }

    private void SetupWeaponButtons()
    {
        UI_InGame inGameUI = UI.instance.inGameUI;

        switchWeapon1Action = () => EquipWeapon(0);
        switchWeapon2Action = () => EquipWeapon(1);
        shootAction = () => combat.StartShooting();
        reloadAction = () =>
        {
            if (combat.CanReload())
            {
                combat.Reload();
            }
        };

        inGameUI.SwitchWeaponButton1.onClick.AddListener(switchWeapon1Action);
        inGameUI.SwitchWeaponButton2.onClick.AddListener(switchWeapon2Action);
        inGameUI.ShootButton.onClick.AddListener(shootAction);
        inGameUI.ReloadButton.onClick.AddListener(reloadAction);
    }

    private void OnDestroy()
    {
        CancelInvoke(); // 대기 중인 AssignUIButtonEvents 재귀 Invoke 정리
        RemoveWeaponButtonListeners();
    }

    private void RemoveWeaponButtonListeners()
    {
        if (UI.instance == null || UI.instance.inGameUI == null)
            return;

        UI_InGame inGameUI = UI.instance.inGameUI;

        if (switchWeapon1Action != null)
            inGameUI.SwitchWeaponButton1.onClick.RemoveListener(switchWeapon1Action);
        if (switchWeapon2Action != null)
            inGameUI.SwitchWeaponButton2.onClick.RemoveListener(switchWeapon2Action);
        if (shootAction != null)
            inGameUI.ShootButton.onClick.RemoveListener(shootAction);
        if (reloadAction != null)
            inGameUI.ReloadButton.onClick.RemoveListener(reloadAction);
    }

}
