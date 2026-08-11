using UnityEngine;

public enum EquipType
{
    SideEquip,
    BackEquip,
}
// 값이 애니메이터 레이어 인덱스로 그대로 사용됨 (PlayerWeaponVisuals 참고)
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
