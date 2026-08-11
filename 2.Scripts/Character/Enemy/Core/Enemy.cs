using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Melee, Range, Boss}
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public LayerMask whatIsAlly;

    public Player player { get; private set; }
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public EnemyStateMachine stateMachine { get; private set; }
    public Enemy_Visuals visuals { get; private set; }
    public Enemy_Health health { get; private set; }
    public Enemy_Ragdoll ragdoll { get; private set; }
    public Enemy_Loot dropController { get; private set; }

    public Enemy_AI ai { get; private set; }
    public Enemy_Movement movement { get; private set; }

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();

        health = GetComponent<Enemy_Health>();
        ragdoll = GetComponent<Enemy_Ragdoll>();
        visuals = GetComponent<Enemy_Visuals>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        dropController = GetComponent<Enemy_Loot>();
        ai = GetComponent<Enemy_AI>();
        movement = GetComponent<Enemy_Movement>();
    }

    protected virtual void Start()
    {
        player = GameManager.GetPlayer();

        movement.InitializePatrolPoints();
    }

    protected virtual void Update()
    {
        // 사망한 적이 전투 진입 경로로 상태를 이탈(부활)하지 않도록 차단
        if (health.IsDead)
            return;

        if (ai.CanEnterBattleMode())
        {
            EnterBattleMode();
        }
    }

    // 플레이어 사망 시 공통 처리 — true를 반환하면 호출부는 이후 로직을 중단해야 함
    protected bool HandlePlayerDeath(EnemyState idleState, EnemyState deadState)
    {
        if (player == null || !player.health.isDead)
            return false;

        if (stateMachine.currentState == deadState)
            stateMachine.currentState.Update();
        else if (stateMachine.currentState != idleState)
            stateMachine.ChangeState(idleState);

        return true;
    }

    protected virtual void InitializePerk() { }

    public virtual void Die()
    {
        GameEvents.RaiseAnyEnemyDied(this);
    }

    public virtual void EnterBattleMode() => ai.EnterBattleMode();

    public void AnimationTrigger() => stateMachine.currentState.AnimationTrigger();

    public virtual void AbilityTrigger() => stateMachine.currentState.AbilityTrigger();

    protected virtual void OnDrawGizmos()
    {
    }

}
