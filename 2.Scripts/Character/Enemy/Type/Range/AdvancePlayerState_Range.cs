using UnityEngine;

public class AdvancePlayerState_Range : EnemyState
{
    private Enemy_Range enemy;
    private Vector3 playerPos;
    private float lastTimeUpdatedDestination;

    private const float DESTINATION_UPDATE_INTERVAL = 0.25f;
    public AdvancePlayerState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.visuals.EnableIK(true, true);

        enemy.agent.isStopped = false;
        enemy.agent.speed = enemy.advanceSpeed;
    }

    public override void Update()
    {
        base.Update();
        playerPos = enemy.player.transform.position;
        enemy.UpdateAimPosition();

        if (CanUpdateDestination())
        {
            enemy.agent.SetDestination(playerPos);
        }

        enemy.movement.FaceTarget(GetNextPathPoint());

        if (CanEnterBattleState() && enemy.IsSeeingPlayer())
            stateMachine.ChangeState(enemy.battleState);
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

    private bool CanEnterBattleState()
    {
        bool isCloseEnough = Vector3.Distance(enemy.transform.position, playerPos) < enemy.advanceStoppingDistance;
        bool isInAggressionRange = enemy.ai.IsPlayerInAggressionRange();

        return isCloseEnough && isInAggressionRange;
    }
}
