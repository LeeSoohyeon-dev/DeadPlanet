using UnityEngine;

public enum EquipType
{
    SideEquip,
    BackEquip,
}
public enum HoldType
{
    CommonHold = 1,
    HighHold = 2,
    LowHold = 3,
    HandgunHold = 4,
}

public class WeaponModel : MonoBehaviour
{
    public WeaponType weaponType;
    public EquipType equipAnimationType;
    public HoldType holdType;

    public Transform bulletSpawnPoint;
    public Transform holdPoint;
}
