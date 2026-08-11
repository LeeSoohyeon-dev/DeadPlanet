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
        currentSpread = baseSpread;

        reloadSpeed = weaponData.reloadSpeed;
        equipmentSpeed = weaponData.equipmentSpeed;
        gunDistance = weaponData.gunDistance;

        this.weaponData = weaponData;
    }

    public Vector3 ApplySpread(Vector3 originalDirection)
    {
        UpdateSpread();

        // 탑다운: 탄환은 수평면을 유지해야 하므로 yaw만 적용 (pitch는 y 성분을 재도입함)
        float yaw = Random.Range(-currentSpread, currentSpread);

        Quaternion spreadRotation = Quaternion.Euler(0f, yaw, 0f);

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

    private bool IsReadyToFire() => Time.time > lastShootTime + 1 / fireRate;

    // 연사 간격 타이머는 실제로 발사된 시점에만 소비 (판정 CanShoot과 분리)
    public void RecordShot() => lastShootTime = Time.time;

    public bool CanReload() => bulletsInMagazine != magazineCapacity && totalReserveAmmo > 0;
    public void RefillBullets()
    {
        int bulletsNeeded = magazineCapacity - bulletsInMagazine;
        int bulletsToReload = Mathf.Min(bulletsNeeded, totalReserveAmmo);

        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine += bulletsToReload;
    }
    private bool HasEnoughBullets() => bulletsInMagazine > 0;

}
