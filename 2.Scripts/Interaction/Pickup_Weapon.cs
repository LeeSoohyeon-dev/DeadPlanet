using UnityEngine;
using VInspector;

public class Pickup_Weapon : Interactable
{
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private Weapon weapon;

    [SerializeField] private BackupWeaponModel[] models;

    private bool oldWeapon;
    private bool hasStarted;

    protected override void Start()
    {
        base.Start();

        hasStarted = true;

        if (oldWeapon == false)
            weapon = WeaponFactory.CreateWeapon(weaponData);

        SetupGameObject();
    }

    private void OnEnable()
    {
        // 풀 재사용 시 Start가 다시 실행되지 않으므로,
        // 이전 픽업의 무기 인스턴스가 남지 않도록 새로 생성
        if (hasStarted == false)
            return;

        oldWeapon = false;
        weapon = WeaponFactory.CreateWeapon(weaponData);
        SetupGameObject();
    }

    public void SetupPickupWeapon(Weapon weapon)
    {
        oldWeapon = true;

        this.weapon = weapon;
        weaponData = weapon.weaponData;

        SetupGameObject();
    }

    [ContextMenu("Update Item Model")]
    public void SetupGameObject()
    {
        SetupWeaponModel();
    }

    [Button]
    private void SetupWeaponModel()
    {
        foreach (BackupWeaponModel model in models)
        {
            model.gameObject.SetActive(false);

            if (model.weaponType == weaponData.weaponType)
            {
                model.gameObject.SetActive(true);
                UpdateMeshAndMaterials(model.GetComponent<MeshRenderer>());
            }
        }
    }

    public override void Interaction()
    {
        if (weapon == null || weaponController == null)
            return;

        weaponController.PickupWeapon(weapon);

        HighlightActive(false);

        ObjectPool.instance.ReturnObject(gameObject);
    }
}
