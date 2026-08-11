using UnityEngine;

public class AttackState_Boss : EnemyState
{
    private Enemy_Boss enemy;
    public AttackState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Boss;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.anim.SetFloat("AttackAnimIndex", Random.Range(0, 2));
        enemy.agent.isStopped = true;

        // 공격 초반에만 플레이어 방향으로 회전 보정
        stateTimer = 1f;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer > 0)
            enemy.movement.FaceTarget(enemy.player.transform.position, 20);

        if (hasTriggerCalled)
        {
            if (enemy.IsPlayerInAttackRange())
                stateMachine.ChangeState(enemy.idleState);
            else
                stateMachine.ChangeState(enemy.moveState);
        }

    }

    public override void Exit()
    {
        base.Exit();
    }
}
