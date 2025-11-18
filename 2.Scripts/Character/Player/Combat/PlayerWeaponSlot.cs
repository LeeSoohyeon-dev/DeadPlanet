using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSlot : MonoBehaviour
{
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private List<Weapon> weaponSlots;
    [SerializeField] private GameObject weaponPickupPrefab;

    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    public void InitializeSlots(Weapon_Data defaultWeaponData)
    {
        weaponSlots[0] = WeaponFactory.CreateWeapon(defaultWeaponData);
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

    public void PickupWeapon(Weapon newWeapon)
    {
        if (WeaponInSlots(newWeapon.weaponType) != null)
        {
            WeaponInSlots(newWeapon.weaponType).totalReserveAmmo += newWeapon.bulletsInMagazine;
            return;
        }

        if (weaponSlots.Count >= maxSlots)
        {
            CreateWeaponOnTheGround(weaponSlots[0]);
            weaponSlots[0] = newWeapon;
            return;
        }

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
        droppedWeapon.GetComponent<Pickup_Weapon>()?.SetupPickupWeapon(weaponToDrop, transform);
    }

    public void ReplaceWeaponAtSlot(int slotIndex, Weapon newWeapon, Weapon currentWeapon)
    {
        if (slotIndex >= weaponSlots.Count)
        {
            return;
        }

        player.weaponVisuals.SwitchOffWeaponModels();
        weaponSlots[slotIndex] = newWeapon;
        CreateWeaponOnTheGround(currentWeapon);
    }
}
