using UnityEngine;

// 애니메이션 클립의 Animation Event에서 호출됨 — 코드 참조가 없어도 삭제/이름 변경 금지
public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerWeaponVisuals visualController;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        visualController = GetComponentInParent<PlayerWeaponVisuals>();
        weaponController = GetComponentInParent<PlayerWeaponController>();
    }

    public void ReloadIsOver()
    {

        visualController.RecoverRigWeight();
        weaponController.CurrentWeapon().RefillBullets();
        weaponController.SetWeaponReady(true);
        weaponController.UpdateWeaponUI();
    }

    public void ReturnRigWeight()
    {
        visualController.RecoverLeftHandIKWeight();
        visualController.RecoverRigWeight();
    }
    public void WeaponEquipingIsOver()
    {
        weaponController.SetWeaponReady(true);
    }

    public void SwitchOnWeaponModel() => visualController.SwitchOnCurrentWeaponModel();

}
