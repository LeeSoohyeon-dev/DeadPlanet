using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Enemy data/Range Weapon Data")]

public class Enemy_RangeWeaponData : ScriptableObject
{
    [Header("Weapon details")]
    public Enemy_RangeWeaponType weaponType;
    public float fireRate = 1f;

    public int minBulletsPerAttack = 1;
    public int maxBulletsPerAttack = 1;

    public float minWeaponCooldown = 2;
    public float maxWeaponCooldown = 3;

    [Header("Bullet details")]
    public int bulletDamage;
    public float bulletSpeed = 20;
    public float weaponSpread = .1f;

    public int GetBulletsPerAttack() => Random.Range(minBulletsPerAttack, maxBulletsPerAttack + 1);
    public float GetWeaponCooldown() => Random.Range(minWeaponCooldown,maxWeaponCooldown);

    public Vector3 ApplyWeaponSpread(Vector3 originalDirection)
    {
        float randomizedValue = Random.Range(-weaponSpread,weaponSpread);
        Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue / 2, randomizedValue);

        return spreadRotation * originalDirection;
    }
}
