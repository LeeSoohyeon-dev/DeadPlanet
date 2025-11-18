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
        if (!IsWeaponReady()) return;

        Weapon currentWeapon = player.weaponController.CurrentWeapon();
        if (currentWeapon.CanShoot() == false) return;

        player.weaponVisuals.PlayFireAnimation();
        GameEvents.OnPlaySound?.Invoke(SoundType.Shoot);

        isShooting = false;
        FireWeapon();
    }

    private void FireWeapon()
    {
        Weapon currentWeapon = player.weaponController.CurrentWeapon();

        if (currentWeapon.bulletsInMagazine <= 0)
            return;

        for (int i = 0; i < currentWeapon.bulletsPerShot; i++)
        {
            CreateBullet();
        }

        currentWeapon.bulletsInMagazine--;
        player.weaponController.UpdateWeaponUI();
    }

    private void CreateBullet()
    {
        Weapon currentWeapon = player.weaponController.CurrentWeapon();

        GameObject newBullet = ObjectPool.instance.GetObject(bulletPrefab, transform);
        newBullet.transform.position = player.weaponController.GunPoint().position;

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
        GameEvents.OnPlaySound?.Invoke(SoundType.Reload);
    }

    public bool CanReload()
    {
        Weapon currentWeapon = player.weaponController.CurrentWeapon();
        return currentWeapon.CanReload() && IsWeaponReady();
    }
}
