using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon System/Weapon Data")]
public class Weapon_Data : ScriptableObject
{
    public string weaponName;

    public int bulletDamage;

    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    public int bulletsPerShot = 1;
    public float fireRate;
    public float baseSpread;
    public float maxSpread;
    public float spreadIncreaseRate = .15f;
    public WeaponType weaponType;
    public float reloadSpeed = 1;
    public float equipmentSpeed = 1;
    public float gunDistance = 20;

    public Sprite weaponIcon;
}
