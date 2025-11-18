using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Melee, Range, Boss}
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    public LayerMask whatIsAlly;
    public int healthPoints = 20;

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
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            player = GameManager.instance.player;
        }
        else
        {
            player = FindFirstObjectByType<Player>();
        }

        movement.InitializePatrolPoints();
    }

    protected virtual void Update()
    {
        if (ai.CanEnterBattleMode())
        {
            EnterBattleMode();
        }
    }

    protected virtual void InitializePerk() { }

    public virtual void Die()
    {
        GameEvents.OnAnyEnemyDied?.Invoke(this);
    }

    public virtual void EnterBattleMode() => ai.EnterBattleMode();

    public void AnimationTrigger() => stateMachine.currentState.AnimationTrigger();

    public virtual void AbilityTrigger() => stateMachine.currentState.AbilityTrigger();

    protected virtual void OnDrawGizmos()
    {
    }

}
