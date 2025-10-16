using UnityEngine;

public enum Enemy_RangeWeaponHoldType
{
    CommonHold, // 그 외
    LowHold, // 어설트 라이플
    HandgunHold, // 권총
}
public class Enemy_RangeWeaponModel : MonoBehaviour
{
    public Transform gunPoint;
    public Enemy_RangeWeaponType weaponType;
    public Enemy_RangeWeaponHoldType weaponHoldType;

    public Transform leftHandIKTarget;
    public Transform leftElbowIKTarget;

}