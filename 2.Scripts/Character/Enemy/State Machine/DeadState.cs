using UnityEngine;

// 사망 상태 공통 처리: 래그돌 연출 후 물리/피격 판정 비활성화
public class DeadState : EnemyState
{
    private bool isInteractionDisabled;

    private const float RAGDOLL_SETTLE_DURATION = 1.5f;

    public DeadState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        isInteractionDisabled = false;

        enemyBase.anim.enabled = false;
        enemyBase.agent.isStopped = true;

        // 공격 애님 도중 사망하면 종료 이벤트가 오지 않아 판정이 켜진 채 남음
        enemyBase.ai.EnableMeleeAttackCheck(false);

        enemyBase.ragdoll.RagdollActive(true);

        stateTimer = RAGDOLL_SETTLE_DURATION;
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
            enemyBase.ragdoll.RagdollActive(false);
            enemyBase.ragdoll.CollidersActive(false);
        }
    }
}
