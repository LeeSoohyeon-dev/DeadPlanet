using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private Weapon_Data defaultWeaponData;
    [SerializeField] private Weapon currentWeapon;

    private Player player;
    private PlayerWeaponSlot weaponSlot;
    private PlayerCombat combat;

    private void Start()
    {
        player = GetComponent<Player>();
        weaponSlot = GetComponent<PlayerWeaponSlot>();
        combat = GetComponent<PlayerCombat>();

        AssignUIButtonEvents();
        Invoke(nameof(EquipStartingWeapon), 0.1f);
    }

    public void UpdateWeaponUI()
    {
        GameEvents.OnWeaponUIUpdate?.Invoke(weaponSlot.GetWeaponSlots(), currentWeapon);
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
    public List<Weapon> GetWeaponSlots() => weaponSlot.GetWeaponSlots();
    private void EquipWeapon(int i)
    {
        List<Weapon> slots = weaponSlot.GetWeaponSlots();
        if (i >= slots.Count)
        {
            return;
        }

        SetWeaponReady(false);
        currentWeapon = slots[i];
        player.weaponVisuals.PlayWeaponEquipAnimation();

        GameEvents.OnPlaySound?.Invoke(SoundType.WeaponSwitch);

        UpdateWeaponUI();
    }

    public void PickupWeapon(Weapon newWeapon)
    {
        if (WeaponInSlots(newWeapon.weaponType) != null)
        {
            WeaponInSlots(newWeapon.weaponType).totalReserveAmmo += newWeapon.bulletsInMagazine;
            return;
        }

        List<Weapon> slots = weaponSlot.GetWeaponSlots();
        if (slots.Count >= 2 && newWeapon.weaponType != currentWeapon.weaponType)
        {
            int weaponIndex = slots.IndexOf(currentWeapon);
            weaponSlot.ReplaceWeaponAtSlot(weaponIndex, newWeapon, currentWeapon);
            EquipWeapon(weaponIndex);
            return;
        }

        weaponSlot.PickupWeapon(newWeapon);
        UpdateWeaponUI();
    }

    public Weapon CurrentWeapon() => currentWeapon;

    public Transform GunPoint() => player.weaponVisuals.CurrentWeaponModel().bulletSpawnPoint;

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

        inGameUI.SwitchWeaponButton1.onClick.AddListener(() => EquipWeapon(0));
        inGameUI.SwitchWeaponButton2.onClick.AddListener(() => EquipWeapon(1));
        inGameUI.ShootButton.onClick.AddListener(() => combat.StartShooting());
        inGameUI.ReloadButton.onClick.AddListener(() =>
        {
            if (combat.CanReload())
            {
                combat.Reload();
            }
        });
    }

}
