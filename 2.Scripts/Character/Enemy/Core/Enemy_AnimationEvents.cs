using UnityEngine;
using VInspector;

public class Enemy_AnimationEvents : MonoBehaviour
{
    private Enemy enemy;
    private Enemy_Melee enemyMelee;
    private Enemy_Boss enemyBoss;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        enemyMelee = GetComponentInParent<Enemy_Melee>();
        enemyBoss = GetComponentInParent<Enemy_Boss>();
    }

    public void AnimationTrigger() => enemy.AnimationTrigger();

    public void StartManualMovement() => enemy.movement.ActivateManualMovement(true);

    public void StopManualMovement() => enemy.movement.ActivateManualMovement(false);

    public void StartManualRotation() => enemy.movement.ActivateManualRotation(true);
    public void StopManualRotation() => enemy.movement.ActivateManualRotation(false);

    public void AbilityEvent() => enemy.AbilityTrigger();
    public void EnableIK() => enemy.visuals.EnableIK(true, true, 1f);

    public void EnableWeaponModel()
    {
        enemy.visuals.EnableWeaponModel(true);
        enemy.visuals.EnableSecondaryWeaponModel(false);
    }

    public void BossJumpImpact()
    {
        enemyBoss?.JumpImpact();
    }

    public void BeginMeleeAttackCheck()
    {
        enemy?.ai.EnableMeleeAttackCheck(true);
        GameEvents.OnPlaySound?.Invoke(SoundType.EnemySlash);
    }

    public void FinishMeleeAttackCheck()
    {
        enemy?.ai.EnableMeleeAttackCheck(false);
    }
}
