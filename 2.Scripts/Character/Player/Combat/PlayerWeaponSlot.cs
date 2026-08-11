using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSlot : MonoBehaviour
{
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private GameObject weaponPickupPrefab;

    // 슬롯은 런타임 생성 전용 (Weapon은 인스펙터 직렬화로 만들 수 없음)
    private readonly List<Weapon> weaponSlots = new List<Weapon>();

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public void InitializeSlots(Weapon_Data defaultWeaponData)
    {
        weaponSlots.Clear();

        Weapon startingWeapon = WeaponFactory.CreateWeapon(defaultWeaponData);
        if (startingWeapon != null)
            weaponSlots.Add(startingWeapon);
    }

    public List<Weapon> GetWeaponSlots() => weaponSlots;

    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        foreach (Weapon weapon in weaponSlots)
        {
            if (weapon.weaponType == weaponType)
            {
                return weapon;
            }
        }
        return null;
    }

    public bool HasOnlyOneWeapon() => weaponSlots.Count <= 1;

    public bool IsFull => weaponSlots.Count >= maxSlots;

    public bool TryMergeAmmo(Weapon newWeapon)
    {
        Weapon existing = WeaponInSlots(newWeapon.weaponType);
        if (existing == null)
            return false;

        // 자기 자신과의 병합(동일 인스턴스)은 탄약 증식이므로 무시
        if (existing == newWeapon)
            return true;

        existing.totalReserveAmmo += newWeapon.bulletsInMagazine + newWeapon.totalReserveAmmo;
        return true;
    }

    public void AddWeapon(Weapon newWeapon)
    {
        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModel();
    }

    public void DropCurrentWeapon(Weapon currentWeapon)
    {
        if (HasOnlyOneWeapon())
        {
            return;
        }

        CreateWeaponOnTheGround(currentWeapon);
        weaponSlots.Remove(currentWeapon);
    }

    private void CreateWeaponOnTheGround(Weapon weaponToDrop)
    {
        GameObject droppedWeapon = ObjectPool.instance.GetObject(weaponPickupPrefab, player.weaponVisuals.dropPoint);
        droppedWeapon.GetComponent<Pickup_Weapon>()?.SetupPickupWeapon(weaponToDrop);
    }

    public void ReplaceWeaponAtSlot(int slotIndex, Weapon newWeapon, Weapon currentWeapon)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Count)
        {
            return;
        }

        player.weaponVisuals.SwitchOffWeaponModels();
        weaponSlots[slotIndex] = newWeapon;
        CreateWeaponOnTheGround(currentWeapon);
    }
}
