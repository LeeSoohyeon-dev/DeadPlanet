using UnityEngine;

public class ThrowGrenadeState_Range : EnemyState
{
    private Enemy_Range enemy;

    // 투척 모션에 진입했지만 아직 수류탄이 손을 떠나지 않은 상태
    public bool isThrowInProgress { get; private set; }

    public ThrowGrenadeState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();

        isThrowInProgress = true;

        enemy.visuals.EnableWeaponModel(false);
        enemy.visuals.EnableIK(false, false);
        enemy.visuals.EnableSecondaryWeaponModel(true);
        enemy.visuals.EnableGrenadeModel(true);
    }

    public override void Update()
    {
        base.Update();

        Vector3 playerPos = enemy.player.transform.position + Vector3.up;

        enemy.movement.FaceTarget(playerPos);
        enemy.aim.position = playerPos;

        if (hasTriggerCalled)
        {
            // Exit에서 리셋하면 사망 전이 시 DeadState.Enter가 읽기 전에 지워지므로 여기서만 리셋
            isThrowInProgress = false;
            stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();
        isThrowInProgress = false;
        enemy.ThrowGrenade();
    }
}
