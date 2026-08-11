using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponVisuals : MonoBehaviour
{
    private Player player;
    private Animator animator;

    [SerializeField] public Transform dropPoint;

    private WeaponModel[] weaponModels;
    private BackupWeaponModel[] backupWeaponModels;

    [Header("Rig")]
    [SerializeField] private float rigWeightRecoveryRate;
    private bool isRigWeightRecovering;
    private Rig rig;

    [Header("Left Hand IK")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIKTarget;
    [SerializeField] private float leftHandIKWeightRecoverRate;
    private bool isLeftHandIKWeightRecovering;

    private void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }
    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();

    }

    public WeaponModel CurrentWeaponModel()
    {
        WeaponType weaponType = player.weaponController.CurrentWeapon().weaponType;

        foreach (WeaponModel model in weaponModels)
        {
            if (model.weaponType == weaponType)
                return model;
        }

        Debug.LogError($"{name}: {weaponType} 타입의 WeaponModel이 자식에 없습니다.");
        return null;
    }

    public void PlayFireAnimation() => animator.SetTrigger("Fire");

    public void PlayReloadAnimation()
    {
        float reloadSpeed = player.weaponController.CurrentWeapon().reloadSpeed;

        animator.SetTrigger("Reload");
        animator.SetFloat("ReloadSpeed", reloadSpeed);

        ReduceRigWeight();
    }

    public void PlayWeaponEquipAnimation()
    {
        WeaponModel weaponModel = CurrentWeaponModel();
        if (weaponModel == null)
            return;

        EquipType equipType = weaponModel.equipAnimationType;

        float equipmentSpeed = player.weaponController.CurrentWeapon().equipmentSpeed;

        leftHandIK.weight = 0f;
        ReduceRigWeight();
        animator.SetTrigger("EquipWeapon");
        animator.SetFloat("EquipType", (float)equipType);
        animator.SetFloat("EquipSpeed", equipmentSpeed);

    }

    public void SwitchOnCurrentWeaponModel()
    {
        WeaponModel weaponModel = CurrentWeaponModel();
        if (weaponModel == null)
            return;

        int animationIndex = (int)weaponModel.holdType;

        SwitchOffWeaponModels();

        SwitchOffBackupWeaponModels();

        if (!player.weaponController.HasOnlyOneWeapon())
        {
            SwitchOnBackupWeaponModel();
        }
        SwitchAnimationLayer(animationIndex);
        weaponModel.gameObject.SetActive(true);
        AttachLeftHandIKTarget(weaponModel);

    }

    public void SwitchOffWeaponModels()
    {
        foreach (WeaponModel model in weaponModels)
        {
            model.gameObject.SetActive(false);
        }
    }

    private void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel model in backupWeaponModels)
        {
            model.Activate(false);
        }
    }

    public void SwitchOnBackupWeaponModel()
    {
        SwitchOffBackupWeaponModels();

        BackupWeaponModel lowHangWeapon = null;
        BackupWeaponModel backHangWeapon = null;
        BackupWeaponModel sideHangWeapon = null;

        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {

            if (backupModel.weaponType == player.weaponController.CurrentWeapon().weaponType)
                continue;

            if (player.weaponController.WeaponInSlots(backupModel.weaponType) != null)
            {
                if (backupModel.HangTypeIs(HangType.LowBackHang))
                    lowHangWeapon = backupModel;

                if(backupModel.HangTypeIs(HangType.BackHang))
                    backHangWeapon = backupModel;

                if(backupModel.HangTypeIs(HangType.SideHang))
                    sideHangWeapon = backupModel;
            }
        }

        lowHangWeapon?.Activate(true);
        backHangWeapon?.Activate(true);
        sideHangWeapon?.Activate(true);
    }

    public void SwitchAnimationLayer(int layerIndex)
    {
        for (int i = 1; i < animator.layerCount; i++)
        {
            animator.SetLayerWeight(i, 0);
        }
        animator.SetLayerWeight(layerIndex, 1);
    }

    private void AttachLeftHandIKTarget(WeaponModel weaponModel)
    {
        Transform targetTransform = weaponModel.holdPoint;
        leftHandIKTarget.localPosition = targetTransform.localPosition;
        leftHandIKTarget.localRotation = targetTransform.localRotation;
    }

    private void UpdateRigWeight()
    {
        if (isRigWeightRecovering)
        {
            rig.weight += rigWeightRecoveryRate * Time.deltaTime;
            if (rig.weight >= 1f)
            {
                isRigWeightRecovering = false;
            }
        }
    }

    private void UpdateLeftHandIKWeight()
    {
        if (isLeftHandIKWeightRecovering)
        {
            leftHandIK.weight += leftHandIKWeightRecoverRate * Time.deltaTime;
            if (leftHandIK.weight >= 1f)
            {
                isLeftHandIKWeightRecovering = false;
            }
        }
    }

    public void ReduceRigWeight()
    {
        // 장전/교체 애니메이션을 IK가 덮어쓰지 않을 정도의 최소 가중치만 남김
        rig.weight = 0.15f;
    }

    public void RecoverRigWeight() => isRigWeightRecovering = true;
    public void RecoverLeftHandIKWeight() => isLeftHandIKWeightRecovering = true;

}
