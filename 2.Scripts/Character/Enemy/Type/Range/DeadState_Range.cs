using UnityEngine;

public class DeadState_Range : DeadState
{
    private Enemy_Range enemy;

    public DeadState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();

        // 투척 모션 도중(수류탄이 손을 떠나기 전)에 죽은 경우에만 발밑에 수류탄을 떨어뜨림
        if (enemy.grenadePerk == GrenadePerk.CanThrowGrenade && enemy.throwGrenadeState.isThrowInProgress)
            enemy.ThrowGrenade();
    }
}
