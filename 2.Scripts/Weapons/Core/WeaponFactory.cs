using UnityEngine;

public static class WeaponFactory
{
    public static Weapon CreateWeapon(Weapon_Data weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogError("[WeaponFactory] Weapon_Data가 null입니다.");
            return null;
        }

        return new Weapon(weaponData);
    }
}
