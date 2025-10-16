using UnityEngine;

public class Pickup_Ammo : Interactable
{
    [SerializeField]private int ammoAmount = 20;

    public override void Interaction()
    {

        foreach (Weapon weapon in weaponController.GetWeaponSlots())
        {
            weapon.totalReserveAmmo += ammoAmount;
        }

        weaponController.UpdateWeaponUI();
        ObjectPool.instance.ReturnObject(gameObject);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
}
