using UnityEngine;

public enum WeaponType
{
    Pistol,
    Revolver,
    AssaultRifle,
    Shotgun,
    Rifle,
}

[System.Serializable]
public class Weapon
{
    public WeaponType weaponType;
    public int bulletDamage;

    public int bulletsPerShot { get; private set; }

    private float defaultFireRate;
    public float fireRate = 1;
    private float lastShootTime;

    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    public float reloadSpeed { get; private set; }
    public float equipmentSpeed { get; private set; }
    public float gunDistance { get; private set; }

    public Weapon_Data weaponData { get; private set; }

    [Header("Spread ")]
    private float baseSpread = 1;
    private float maximumSpread = 3;
    private float currentSpread = 2;

    private float spreadIncreaseRate = .15f;

    private float lastSpreadUpdateTime;
    private float spreadCooldown = 1;

    public Weapon(Weapon_Data weaponData)
    {
        bulletDamage = weaponData.bulletDamage;
        bulletsInMagazine = weaponData.bulletsInMagazine;
        magazineCapacity = weaponData.magazineCapacity;
        totalReserveAmmo = weaponData.totalReserveAmmo;

        fireRate = weaponData.fireRate;
        weaponType = weaponData.weaponType;

        bulletsPerShot = weaponData.bulletsPerShot;

        baseSpread = weaponData.baseSpread;
        maximumSpread = weaponData.maxSpread;
        spreadIncreaseRate = weaponData.spreadIncreaseRate;

        reloadSpeed = weaponData.reloadSpeed;
        equipmentSpeed = weaponData.equipmentSpeed;
        gunDistance = weaponData.gunDistance;

        defaultFireRate = fireRate;
        this.weaponData = weaponData;
    }

    public Vector3 ApplySpread(Vector3 originalDirection)
    {
        UpdateSpread();

        float randomizedValue = Random.Range(-currentSpread, currentSpread);

        Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue / 2, randomizedValue);

        return spreadRotation * originalDirection;
    }

    private void UpdateSpread()
    {
        if (Time.time > lastSpreadUpdateTime + spreadCooldown)
            currentSpread = baseSpread;
        else
            IncreaseSpread();

        lastSpreadUpdateTime = Time.time;
    }

    private void IncreaseSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread + spreadIncreaseRate, baseSpread, maximumSpread);
    }

    public bool CanShoot() => HasEnoughBullets() && IsReadyToFire();

    private bool IsReadyToFire()
    {
        if (Time.time > lastShootTime + 1 / fireRate)
        {
            lastShootTime = Time.time;
            return true;
        }

        return false;
    }

    public bool CanReload()
    {
        if (bulletsInMagazine == magazineCapacity)
            return false;

        if (totalReserveAmmo > 0)
        {
            return true;
        }

        return false;
    }
    public void RefillBullets()
    {
        int bulletsNeeded = magazineCapacity - bulletsInMagazine;
        int bulletsToReload = Mathf.Min(bulletsNeeded, totalReserveAmmo);

        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine += bulletsToReload;
    }
    private bool HasEnoughBullets() => bulletsInMagazine > 0;

}
