using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private LayerMask whatIsAlly;
    private const float REFERENCE_BULLET_SPEED = 20f;

    [Header("Bullet")]
    [SerializeField] private float bulletImpactForce;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;

    private Player player;
    private bool isShooting;
    private bool isWeaponReady;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player.health.isDead)
        {
            isShooting = false;
            return;
        }

        if (isShooting)
        {
            Shoot();
        }
    }

    public void SetWeaponReady(bool ready) => isWeaponReady = ready;
    public bool IsWeaponReady() => isWeaponReady;

    public void StartShooting() => isShooting = true;

    private void Shoot()
    {
        // 준비 전에 눌린 입력은 버림
        if (!IsWeaponReady())
        {
            isShooting = false;
            return;
        }

        Weapon currentWeapon = player.weaponController.CurrentWeapon();
        if (currentWeapon == null || currentWeapon.CanShoot() == false)
        {
            // 탄약이 없으면 입력을 버림 (재장전 완료 순간 자동 발사 방지)
            if (currentWeapon == null || currentWeapon.bulletsInMagazine <= 0)
                isShooting = false;

            return;
        }

        player.weaponVisuals.PlayFireAnimation();
        GameEvents.RaisePlaySound(SoundType.Shoot);

        isShooting = false;
        FireWeapon(currentWeapon);
    }

    private void FireWeapon(Weapon currentWeapon)
    {
        if (currentWeapon.bulletsInMagazine <= 0)
            return;

        Transform gunPoint = player.weaponController.GunPoint();
        if (gunPoint == null)
            return;

        for (int i = 0; i < currentWeapon.bulletsPerShot; i++)
        {
            CreateBullet(currentWeapon, gunPoint);
        }

        currentWeapon.bulletsInMagazine--;
        currentWeapon.RecordShot();
        player.weaponController.UpdateWeaponUI();
    }

    private void CreateBullet(Weapon currentWeapon, Transform gunPoint)
    {
        GameObject newBullet = ObjectPool.instance.GetObject(bulletPrefab, transform);
        newBullet.transform.position = gunPoint.position;

        int finalDamage = currentWeapon.bulletDamage;

        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.BulletSetup(whatIsAlly, finalDamage, currentWeapon.gunDistance, bulletImpactForce);

        Vector3 bulletDirection = currentWeapon.ApplySpread(player.aim.BulletDirection());

        newBullet.transform.rotation = Quaternion.LookRotation(bulletDirection);

        bulletScript.SetVelocity(bulletDirection, bulletSpeed, REFERENCE_BULLET_SPEED);
    }

    public void Reload()
    {
        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();
        GameEvents.RaisePlaySound(SoundType.Reload);
    }

    public bool CanReload()
    {
        Weapon currentWeapon = player.weaponController.CurrentWeapon();
        return currentWeapon.CanReload() && IsWeaponReady();
    }
}
