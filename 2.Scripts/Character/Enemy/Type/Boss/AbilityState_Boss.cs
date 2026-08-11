using UnityEngine;

public class AbilityState_Boss : EnemyState
{
    private Enemy_Boss enemy;

    // 애님 이벤트가 유실됐을 때 상태에 영구히 갇히지 않기 위한 탈출 여유 시간
    private const float EXIT_TIMEOUT = 3f;

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

        if (stateTimer < -EXIT_TIMEOUT)
        {
            stateMachine.ChangeState(enemy.moveState);
            return;
        }

        if (hasTriggerCalled)
            stateMachine.ChangeState(enemy.moveState);
    }

    private bool IsFlamethrowerExpired() => stateTimer < 0;

    public void DisableFlamethrower()
    {
        if (enemy.isFlamethrowActive == false)
            return;

        enemy.ActivateFlamethrower(false);
        GameEvents.RaiseStopSound(SoundType.EnemyFlameThrower);
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        // 화염이 실제로 켜지는 시점부터 지속시간 측정 (애니메이션 준비 구간 제외)
        stateTimer = enemy.flamethrowDuration;

        enemy.ActivateFlamethrower(true);
        GameEvents.RaisePlaySound(SoundType.EnemyFlameThrower);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
