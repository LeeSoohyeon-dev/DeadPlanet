using UnityEngine;

public class BattleState_Range : EnemyState
{
    private Enemy_Range enemy;

    private float lastTimeShot = -10;
    private int bulletsShot = 0;

    private int bulletsPerAttack;
    private float weaponCooldown;

    private bool isFirstTimeAttack = true;
    public BattleState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }
    public override void Enter()
    {
        base.Enter();
        SetupValuesForFirstAttack();

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        enemy.visuals.EnableIK(true, true);

        stateTimer = enemy.attackDelay;
    }

    public override void Update()
    {
        base.Update();

        if (MustAdvancePlayer())
        {
            stateMachine.ChangeState(enemy.advancePlayerState);
            return;
        }

        if (enemy.IsSeeingPlayer())
        {
            enemy.UpdateAimPosition();
            enemy.movement.FaceTarget(enemy.aim.position);
        }

        if (enemy.CanThrowGrenade())
        {
            stateMachine.ChangeState(enemy.throwGrenadeState);
            return;
        }

        if (stateTimer > 0)
            return;

        if (IsWeaponOutOfBullets())
        {
            if (IsCooldownFinished())
                ResetWeaponValues();

            return;
        }

        if (CanShoot() && enemy.IsAimOnPlayer())
        {
            Shoot();
        }
    }

    private bool MustAdvancePlayer()
    {
        return enemy.ai.IsPlayerInAggressionRange() == false;
    }

    // 다음 사격 세트의 발사 수/쿨다운을 다시 굴림
    private void ResetWeaponValues()
    {
        bulletsShot = 0;
        bulletsPerAttack = enemy.weaponData.GetBulletsPerAttack();
        weaponCooldown = enemy.weaponData.GetWeaponCooldown();
    }
    private bool IsCooldownFinished() => Time.time > lastTimeShot + weaponCooldown;
    private bool IsWeaponOutOfBullets() => bulletsShot >= bulletsPerAttack;
    private bool CanShoot() => Time.time > lastTimeShot + 1 / enemy.weaponData.fireRate;
    private void Shoot()
    {
        enemy.FireSingleBullet();
        lastTimeShot = Time.time;
        bulletsShot++;
    }

    private void SetupValuesForFirstAttack()
    {
        if (isFirstTimeAttack)
        {
            isFirstTimeAttack = false;
            ResetWeaponValues();
        }
    }

}
