using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cysharp.Threading.Tasks;

public enum Enemy_RangeWeaponType
{
    Pistol,
    Revolver,
    Shotgun,
    AssaultRifle,
    Rifle,
}

public class Enemy_Visuals : MonoBehaviour
{

    public GameObject currentWeaponModel { get; private set; }
    public GameObject grenadeModel;

    [SerializeField] private GameObject[] visuals;

    [Header("IK")]
    [SerializeField] private Transform leftHandIK;
    [SerializeField] private Transform leftElbowIK;
    [SerializeField] private TwoBoneIKConstraint leftHandIKConstraint;
    [SerializeField] private MultiAimConstraint weaponAimConstraint;
    private float leftHandTargetWeight;
    private float weaponAimTargetWeight;
    private float rigChangeRate;

    private void Update()
    {
        if(leftHandIKConstraint != null)
            leftHandIKConstraint.weight = AdjustIKWeight(leftHandIKConstraint.weight, leftHandTargetWeight);

        if(weaponAimConstraint != null)
            weaponAimConstraint.weight = AdjustIKWeight(weaponAimConstraint.weight, weaponAimTargetWeight);
    }

    public void EnableGrenadeModel(bool active) => grenadeModel?.SetActive(active);
    public void EnableWeaponModel(bool active)
    {
        currentWeaponModel?.gameObject.SetActive(active);
    }

    public void EnableSecondaryWeaponModel(bool active)
    {
        FindSeconderyWeaponModel()?.SetActive(active);
    }

    public void EnableWeaponTrail(bool enable)
    {
        Enemy_WeaponModel currentWeaponModelScript = currentWeaponModel.GetComponent<Enemy_WeaponModel>();
        currentWeaponModelScript.EnableTrailEffect(enable);
    }

    public void SetupLook()
    {
        SetupRandomWeapon();
        SetupRandomVisuals();
    }

    private void SetupRandomWeapon()
    {
        bool thisEnemyIsMelee = GetComponent<Enemy_Melee>() != null;
        bool thisEnemyIsRange = GetComponent<Enemy_Range>() != null;

        if (thisEnemyIsRange)
            currentWeaponModel = FindRangeWeaponModel();

        if (thisEnemyIsMelee)
            currentWeaponModel = FindMeleeWeaponModel();

        if (currentWeaponModel == null)
        {
            Debug.LogWarning($"{gameObject.name}: 무기 모델을 찾을 수 없습니다. SetupRandomWeapon 재시도 중...");

            RetrySetupRandomWeaponAsync().Forget();
            return;
        }

        currentWeaponModel.SetActive(true);

        OverrideAnimatorControllerIfCan();
    }

    private GameObject FindRangeWeaponModel()
    {
        Enemy_RangeWeaponModel[] weaponModels = GetComponentsInChildren<Enemy_RangeWeaponModel>(true);
        Enemy_RangeWeaponType weaponType = GetComponent<Enemy_Range>().weaponType;

        foreach (var weaponModel in weaponModels)
        {
            if (weaponModel.weaponType == weaponType)
            {
                SwitchAnimationLayer(((int)weaponModel.weaponHoldType));
                SetupLeftHandIK(weaponModel.leftHandIKTarget, weaponModel.leftElbowIKTarget);
                return weaponModel.gameObject;
            }
        }

        return null;
    }

    private GameObject FindMeleeWeaponModel()
    {
        Enemy_WeaponModel[] weaponModels = GetComponentsInChildren<Enemy_WeaponModel>(true);
        
        if (weaponModels == null || weaponModels.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 사용 가능한 근접 무기 모델을 찾을 수 없습니다.");
            return null;
        }

        List<Enemy_WeaponModel> availableModels = new List<Enemy_WeaponModel>();

        foreach (var weaponModel in weaponModels)
        {
            if (weaponModel != null && weaponModel.gameObject != null)
            {
                availableModels.Add(weaponModel);
            }
        }

        if (availableModels.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name}: 유효한 근접 무기 모델이 없습니다.");
            return null;
        }

        int randomIndex = Random.Range(0, availableModels.Count);
        return availableModels[randomIndex].gameObject;
    }
    private GameObject FindSeconderyWeaponModel()
    {
        Enemy_SeconoderyRangeWeaponModel[] weaponModels = GetComponentsInChildren<Enemy_SeconoderyRangeWeaponModel>(true);
        Enemy_RangeWeaponType weaponType = GetComponentInParent<Enemy_Range>().weaponType;

        foreach (var weaponModel in weaponModels)
        {
            if (weaponModel.weaponType == weaponType)
                return weaponModel.gameObject;
        }

        return null;
    }
    private void OverrideAnimatorControllerIfCan()
    {
        AnimatorOverrideController overrideController = currentWeaponModel.GetComponent<Enemy_WeaponModel>()?.OverrideController;

        if (overrideController != null)
        {
            GetComponentInChildren<Animator>().runtimeAnimatorController = overrideController;
        }
    }

    private void SetupRandomVisuals()
    {
        foreach (var visual in visuals)
        {
            visual.SetActive(false);
        }
        int randomIndex = Random.Range(0, visuals.Length);
        visuals[randomIndex].SetActive(true);
    }

    private void SwitchAnimationLayer(int layerIndex)
    {
        Animator animator = GetComponentInChildren<Animator>();

        for (int i = 1; i < animator.layerCount; i++)
        {
            animator.SetLayerWeight(i, 0);
        }
        animator.SetLayerWeight(layerIndex, 1);
    }

    public void EnableIK(bool enableLeftHand, bool enableAim,float changeRate = 10)
    {
        if (leftHandIKConstraint == null)
        {
            return;
        }

        rigChangeRate = changeRate;
        leftHandTargetWeight = enableLeftHand ? 1 : 0;
        weaponAimTargetWeight = enableAim ? 1 : 0;
    }

    private void SetupLeftHandIK(Transform leftHandTarget, Transform leftElbowTarget)
    {
        leftHandIK.localPosition = leftHandTarget.localPosition;
        leftHandIK.localRotation = leftHandTarget.localRotation;

        leftElbowIK.localPosition = leftElbowTarget.localPosition;
        leftElbowIK.localRotation = leftElbowTarget.localRotation;

    }

    private float AdjustIKWeight(float currentWeight,float targetWeight)
    {
        if (Mathf.Abs(currentWeight - targetWeight) > 0.05f)
            return Mathf.Lerp(currentWeight, targetWeight, rigChangeRate * Time.deltaTime);
        else
            return targetWeight;
    }

    private async UniTaskVoid RetrySetupRandomWeaponAsync()
    {
        for (int i = 0; i < 5; i++)
        {
            await UniTask.Delay(100, cancellationToken: this.GetCancellationTokenOnDestroy());
            
            bool thisEnemyIsMelee = GetComponent<Enemy_Melee>() != null;
            bool thisEnemyIsRange = GetComponent<Enemy_Range>() != null;

            if (thisEnemyIsRange)
                currentWeaponModel = FindRangeWeaponModel();

            if (thisEnemyIsMelee)
                currentWeaponModel = FindMeleeWeaponModel();

            if (currentWeaponModel != null)
            {
                currentWeaponModel.SetActive(true);
                OverrideAnimatorControllerIfCan();
                return;
            }
        }

        Debug.LogError($"{gameObject.name}: 무기 모델을 찾을 수 없습니다. 재시도 실패.");
    }
}
