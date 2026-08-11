using UnityEngine;

public class JumpAttackState_Boss : EnemyState
{
    private Enemy_Boss enemy;
    private Vector3 lastPlayerPos;

    private float jumpAttackMovementSpeed;
    public JumpAttackState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Boss;
    }

    public override void Enter()
    {
        base.Enter();

        lastPlayerPos = enemy.player.transform.position;
        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        enemy.bossVisuals.PlaceLandindZone(lastPlayerPos);

        float distanceToPlayer = Vector3.Distance(lastPlayerPos, enemy.transform.position);

        jumpAttackMovementSpeed = distanceToPlayer / enemy.travelTimeToTarget;

        enemy.movement.FaceTarget(lastPlayerPos, 1000);

    }

    public override void Update()
    {
        base.Update();

        bool isManualMovement = enemy.movement.IsManualMovementActive();

        if (enemy.agent.enabled == isManualMovement)
            enemy.agent.enabled = !isManualMovement;

        if (isManualMovement)
        {
            enemy.transform.position =
                Vector3.MoveTowards(enemy.transform.position, lastPlayerPos, jumpAttackMovementSpeed * Time.deltaTime);
        }

        if (hasTriggerCalled)
            stateMachine.ChangeState(enemy.moveState);
    }

    public override void Exit()
    {
        base.Exit();

        if (enemy.agent.enabled == false)
            enemy.agent.enabled = true;
    }
}
