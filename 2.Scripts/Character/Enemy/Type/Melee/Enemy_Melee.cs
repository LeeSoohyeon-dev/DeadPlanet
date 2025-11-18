 using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct AttackData_EnemyMelee
{
    public int attackDamage;
    public string attackName;
    public float attackRange;
    public float moveSpeed;
    public float attackIndex;
    [Range(1, 2)]
    public float animationSpeed;
    public AttackType_Melee attackType;
}

public enum AttackType_Melee
{
    Close,
    Charge
}

public enum EnemyMelee_Type
{
    Regular,
    Shield
}

public class Enemy_Melee : Enemy
{

    public IdleState_Melee idleState { get; private set; }
    public MoveState_Melee moveState { get; private set; }
    public RecoveryState_Melee recoveryState { get; private set; }
    public ChaseState_Melee chaseState { get; private set; }
    public AttackState_Melee attackState { get; private set; }
    public DeadState_Melee deadState { get; private set; }

    public EnemyMelee_Type meleeType;

    [Header("Shield")]
    public int shieldDurability;
    public Transform shieldTransform;

    public AttackData_EnemyMelee attackData;
    public List<AttackData_EnemyMelee> attackList;
    private Enemy_WeaponModel currentWeapon;
    [SerializeField] private GameObject meleeAttackFx;

    protected override void Awake()
    {
        base.Awake();

        idleState = new IdleState_Melee(this, stateMachine, "isIdle");
        moveState = new MoveState_Melee(this, stateMachine, "isMoving");
        recoveryState = new RecoveryState_Melee(this, stateMachine, "isRecovering");
        chaseState = new ChaseState_Melee(this, stateMachine, "isChasing");
        attackState = new AttackState_Melee(this, stateMachine, "isAttacking");
        deadState = new DeadState_Melee(this, stateMachine, "isIdle");
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

        InitializePerk();
        visuals.SetupLook();
        UpdateAttackData();
    }

    protected override void Update()
    {
        base.Update();
        if(player.health.isDead)
        {
            stateMachine.ChangeState(idleState);
            return;
        }
        stateMachine.currentState.Update();

        ai.MeleeAttackCheck(currentWeapon.damagePoints, currentWeapon.attackRadius, meleeAttackFx, attackData.attackDamage);
    }

    public override void EnterBattleMode()
    {
        if (ai.isInBattleMode)
            return;

        base.EnterBattleMode();
        stateMachine.ChangeState(recoveryState);
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        movement.walkSpeed = movement.walkSpeed * .6f;
        visuals.EnableWeaponModel(false);
    }

    public void UpdateAttackData()
    {
        currentWeapon = visuals.currentWeaponModel.GetComponent<Enemy_WeaponModel>();

        if (currentWeapon.weaponData != null)
        {
            attackList = new List<AttackData_EnemyMelee>(currentWeapon.weaponData.attackData);
            movement.turnSpeed = currentWeapon.weaponData.turnSpeed;
        }
    }

    protected override void InitializePerk()
    {
        if (meleeType == EnemyMelee_Type.Shield)
        {
            anim.SetFloat("ChaseIndex", 1);
            shieldTransform.gameObject.SetActive(true);
        }
    }

    public override void Die()
    {
        base.Die();

        if(stateMachine.currentState != deadState)
            stateMachine.ChangeState(deadState);
    }

    public bool IsPlayerInAttackRange() => Vector3.Distance(transform.position, player.transform.position) < attackData.attackRange;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);

    }
}
