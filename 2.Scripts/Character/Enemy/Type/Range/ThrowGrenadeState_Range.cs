using UnityEngine;

public class ThrowGrenadeState_Range : EnemyState
{
    private Enemy_Range enemy;
    public bool hasFinishedThrowingGrenade { get; private set; }

    public ThrowGrenadeState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();

        hasFinishedThrowingGrenade = false;

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
            stateMachine.ChangeState(enemy.battleState);
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();
        hasFinishedThrowingGrenade = true;
        enemy.ThrowGrenade();
    }
}
