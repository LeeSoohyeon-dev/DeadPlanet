using UnityEngine;

public class MoveState_Boss : EnemyState
{
    private Enemy_Boss enemy;
    private Vector3 destination;

    private float actionTimer;
    private float lastTimeUpdatedDestination;

    private const float DESTINATION_UPDATE_INTERVAL = 0.25f;

    public MoveState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Boss;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.agent.speed = enemy.movement.GetWalkSpeed();

        enemy.agent.isStopped = false;

        destination = enemy.movement.GetPatrolDestination();
        enemy.agent.SetDestination(destination);

        actionTimer = enemy.specialActionAttemptInterval;
    }

    public override void Update()
    {
        base.Update();

        actionTimer -= Time.deltaTime;
        enemy.movement.FaceTarget(GetNextPathPoint());

        if (enemy.ai.IsInBattleMode())
        {
            if (CanUpdateDestination())
            {
                enemy.agent.SetDestination(enemy.player.transform.position);
            }

            if (actionTimer < 0)
            {
                PerformSpecialAction();
            }
            else if (enemy.IsPlayerInAttackRange())
                stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            if (!enemy.agent.pathPending && enemy.agent.remainingDistance <= enemy.agent.stoppingDistance + 0.05f)
                stateMachine.ChangeState(enemy.idleState);
        }
    }

    private bool CanUpdateDestination()
    {
        if (Time.time > lastTimeUpdatedDestination + DESTINATION_UPDATE_INTERVAL)
        {
            lastTimeUpdatedDestination = Time.time;
            return true;
        }
        return false;
    }

    private void PerformSpecialAction()
    {
        actionTimer = enemy.specialActionAttemptInterval;

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);

        if (distanceToPlayer >= enemy.minJumpDistanceRequired)
        {
            stateMachine.ChangeState(enemy.jumpAttackState);
        }
        else if (distanceToPlayer >= enemy.minFlamethrowerDistance)
        {
            stateMachine.ChangeState(enemy.abilityState);
        }
    }
}
