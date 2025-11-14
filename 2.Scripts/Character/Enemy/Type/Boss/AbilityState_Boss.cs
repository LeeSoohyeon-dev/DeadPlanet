using UnityEngine;

public class AbilityState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    public AbilityState_Boss(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Boss;
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = enemy.flamethrowDuration;

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;
    }

    public override void Update()
    {
        base.Update();

        enemy.movement.FaceTarget(enemy.player.transform.position);

        if (IsFlamethrowerExpired())
            DisableFlamethrower();

        if (hasTriggerCalled)
            stateMachine.ChangeState(enemy.moveState);
    }

    private bool IsFlamethrowerExpired() => stateTimer < 0;

    public void DisableFlamethrower()
    {
        if (enemy.isFlamethrowActive == false)
            return;

        enemy.ActivateFlamethrower(false);
        GameEvents.OnStopSound?.Invoke(SoundType.EnemyFlameThrower);
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        enemy.ActivateFlamethrower(true);
        GameEvents.OnPlaySound?.Invoke(SoundType.EnemyFlameThrower);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
