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

        if (enemy.IsSeeingPlayer())
            enemy.movement.FaceTarget(enemy.aim.position);

        if (enemy.CanThrowGrenade())
            stateMachine.ChangeState(enemy.throwGrenadeState);

        if (MustAdvancePlayer())
            stateMachine.ChangeState(enemy.advancePlayerState);

        if (stateTimer > 0)
            return;

        if (IsWeaponOutOfBullets())
        {
            if (enemy.IsUnstopppable() && IsUnstoppableWalkReady())
            {
                enemy.advanceDuration = weaponCooldown;
                stateMachine.ChangeState(enemy.advancePlayerState);
            }

            if (IsWeaponOnCooldown())
                AttemptToResetWeapon();

            return;
        }


        if (CanShoot() && enemy.IsAimOnPlayer())
        {
            Shoot();
        }
    }

    private bool MustAdvancePlayer()
    {
        if (enemy.IsUnstopppable())
            return false;

        return enemy.ai.IsPlayerInAggressionRange() == false;
    }

    private bool IsUnstoppableWalkReady()
    {
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
        bool isOutOfStoppingDistance = distanceToPlayer > enemy.advanceStoppingDistance;
        bool isUnstoppableWalkOnCooldown =
            Time.time < enemy.weaponData.maxWeaponCooldown + enemy.advancePlayerState.lastTimeAdvanced;

        return isOutOfStoppingDistance && isUnstoppableWalkOnCooldown == false;
    }

    private void AttemptToResetWeapon()
    {
        bulletsShot = 0;
        bulletsPerAttack = enemy.weaponData.GetBulletsPerAttack();
        weaponCooldown = enemy.weaponData.GetWeaponCooldown();
    }
    private bool IsWeaponOnCooldown() => Time.time > lastTimeShot + weaponCooldown;
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
            bulletsPerAttack = enemy.weaponData.GetBulletsPerAttack();
            weaponCooldown = enemy.weaponData.GetWeaponCooldown();
        }
    }

    
}
