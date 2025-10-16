using UnityEngine;

public enum EquipType
{
    SideEquip,
    BackEquip,
}
public enum HoldType
{
    CommonHold = 1, // 라이플
    HighHold = 2, // 샷건
    LowHold = 3, // 어설트 라이플
    HandgunHold = 4, // 피스톨, 리볼버
}

public class WeaponModel : MonoBehaviour
{
    public WeaponType weaponType;
    public EquipType equipAnimationType;
    public HoldType holdType;

    public Transform bulletSpawnPoint;
    public Transform holdPoint;
}
