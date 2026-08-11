using UnityEngine;

public class Enemy_AI : MonoBehaviour
{
    public LayerMask whatIsPlayer;
    public float aggressionRange;
    public float idleTime;

    private Enemy enemy;
    public bool isInBattleMode;
    protected bool isMeleeAttackReady;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public bool CanEnterBattleMode()
    {
        return !isInBattleMode && IsPlayerInAggressionRange();
    }

    public virtual void EnterBattleMode()
    {
        isInBattleMode = true;
    }

    private static readonly Collider[] meleeCheckBuffer = new Collider[8];

    public virtual void MeleeAttackCheck(Transform[] damagePoints, float attackCheckRadius, GameObject fx, int damage)
    {
        if (isMeleeAttackReady == false)
            return;

        foreach (Transform attackPoint in damagePoints)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(attackPoint.position, attackCheckRadius, meleeCheckBuffer, whatIsPlayer);

            for (int i = 0; i < hitCount; i++)
            {
                IDamagable damagable = meleeCheckBuffer[i].GetComponent<IDamagable>();

                if (damagable != null)
                {
                    damagable.TakeDamage(damage);
                    isMeleeAttackReady = false;
                    GameObject newAttackFx = ObjectPool.instance.GetObject(fx, attackPoint);

                    ObjectPool.instance.ReturnObject(newAttackFx, 1);
                    return;
                }
            }
        }
    }

    public void EnableMeleeAttackCheck(bool enable) => isMeleeAttackReady = enable;

    public bool IsPlayerInAggressionRange() =>
        enemy.player != null && Vector3.Distance(transform.position, enemy.player.transform.position) < aggressionRange;

    public bool IsInBattleMode() => isInBattleMode;

    public virtual void GetHit(int damage)
    {
        if (enemy.health.IsDead)
            return;

        enemy.health.ReduceHealth(damage);

        if (enemy.health.CheckAndMarkDeath())
        {
            enemy.dropController?.DropItems();
            enemy.Die();
            return;
        }

        EnterBattleMode();
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, aggressionRange);
    }
}
