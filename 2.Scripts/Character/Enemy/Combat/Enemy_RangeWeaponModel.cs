using UnityEngine;

public enum Enemy_RangeWeaponHoldType
{
    CommonHold,
    LowHold,
    HandgunHold,
}
public class Enemy_RangeWeaponModel : MonoBehaviour
{
    public Transform gunPoint;
    public Enemy_RangeWeaponType weaponType;
    public Enemy_RangeWeaponHoldType weaponHoldType;

    public Transform leftHandIKTarget;
    public Transform leftElbowIKTarget;

}