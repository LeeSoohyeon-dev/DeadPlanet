using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponVisuals : MonoBehaviour
{
    private Player player;
    private Animator animator;

    [SerializeField] public Transform dropPoint;

    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupWeaponModels;

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
        WeaponModel weaponModel = null;

        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;

        foreach (WeaponModel model in weaponModels)
        {
            if (model.weaponType == weaponType)
            {
                weaponModel = model;
                break;
            }
        }

        return weaponModel;
    }

    public void PlayFireAnimation() => animator.SetTrigger("Fire");

    public void PlayReloadAnimation()
    {
        float reloadSpeed = player.weapon.CurrentWeapon().reloadSpeed;

        animator.SetTrigger("Reload");
        animator.SetFloat("ReloadSpeed", reloadSpeed);

        ReduceRigWeight();
    }

    public void PlayWeaponEquipAnimation()
    {
        EquipType equipType = CurrentWeaponModel().equipAnimationType;

        float equipmentSpeed = player.weapon.CurrentWeapon().equipmentSpeed;

        leftHandIK.weight = 0f;
        ReduceRigWeight();
        animator.SetTrigger("EquipWeapon");
        animator.SetFloat("EquipType", (float)equipType);
        animator.SetFloat("EquipSpeed", equipmentSpeed);

    }

    public void SwitchOnCurrentWeaponModel()
    {
        int animationIndex = (int)CurrentWeaponModel().holdType;

        SwitchOffWeaponModels();

        SwitchOffBackupWeaponModels();

        if (!player.weapon.HasOnlyOneWeapon())
        {
            SwitchOnBackupWeaponModel();
        }
        SwitchAnimationLayer(animationIndex);
        CurrentWeaponModel().gameObject.SetActive(true);
        AttachLeftHandIKTarget();

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

            if (backupModel.weaponType == player.weapon.CurrentWeapon().weaponType)
                continue;

            if (player.weapon.WeaponInSlots(backupModel.weaponType) != null)
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

    private void AttachLeftHandIKTarget()
    {
        Transform tragetTransform = CurrentWeaponModel().holdPoint;
        leftHandIKTarget.localPosition = tragetTransform.localPosition;
        leftHandIKTarget.localRotation = tragetTransform.localRotation;
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
        rig.weight = 0.15f;
    }

    public void RecoverRigWeight() => isRigWeightRecovering = true;
    public void RecoverLeftHandIKWeight() => isLeftHandIKWeightRecovering = true;

}
