using UnityEngine;

public class DeadState_Range : EnemyState
{
    private Enemy_Range enemy;
    private bool isInteractionDisabled;

    public DeadState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();

        if (enemy.throwGrenadeState.hasFinishedThrowingGrenade == false)
            enemy.ThrowGrenade();

        isInteractionDisabled = false;

        enemy.anim.enabled = false;
        enemy.agent.isStopped = true;

        enemy.ragdoll.RagdollActive(true);

        stateTimer = 1.5f;
    }

    public override void Update()
    {
        base.Update();

        CheckAndDisableInteraction();
    }

    private void CheckAndDisableInteraction()
    {
        if (stateTimer < 0 && isInteractionDisabled == false)
        {
            isInteractionDisabled = true;
            enemy.ragdoll.RagdollActive(false);
            enemy.ragdoll.CollidersActive(false);
        }
    }
}
