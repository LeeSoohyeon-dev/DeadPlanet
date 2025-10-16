using UnityEngine;
using VInspector;

public class MoveState_Boss : EnemyState
{
    private Enemy_Boss enemy;
    private Vector3 destination;

    private float actionTimer;

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

        actionTimer = enemy.actionCooldown;
    }


    public override void Update()
    {
        base.Update();

        actionTimer -= Time.deltaTime;
        enemy.movement.FaceTarget(GetNextPathPoint());

        if (enemy.ai.IsInBattleMode())
        {


            Vector3 playerPos = enemy.player.transform.position;
            enemy.agent.SetDestination(playerPos);

            if (actionTimer < 0)
            {
                PerformRandomAction();
            }
            else if (enemy.IsPlayerInAttackRange())
                stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            if (Vector3.Distance(enemy.transform.position, destination) < .25f)
                stateMachine.ChangeState(enemy.idleState);
        }
    }



    private void PerformRandomAction()
    {
        actionTimer = enemy.actionCooldown;

        if (Random.Range(0, 2) == 0)
        {
            TryAbility();
        }
        else
        {
            if (enemy.CanDoJumpAttack())
                stateMachine.ChangeState(enemy.jumpAttackState);
        }
    }

    private void TryAbility()
    {
        stateMachine.ChangeState(enemy.abilityState);
    }



}
