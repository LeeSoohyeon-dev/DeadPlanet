using UnityEngine;

public class DeadState_Boss : DeadState
{
    private Enemy_Boss enemy;

    public DeadState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Boss;
    }

    public override void Enter()
    {
        enemy.abilityState.DisableFlamethrower();

        base.Enter();
    }
}
