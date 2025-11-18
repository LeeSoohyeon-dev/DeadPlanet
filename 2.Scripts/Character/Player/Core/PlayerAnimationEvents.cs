using UnityEngine;

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
