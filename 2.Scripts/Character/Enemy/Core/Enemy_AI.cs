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

    public virtual void MeleeAttackCheck(Transform[] damagePoints, float attackCheckRadius, GameObject fx, int damage)
    {
        if (isMeleeAttackReady == false)
            return;

        foreach (Transform attackPoint in damagePoints)
        {
            Collider[] detectedHits = Physics.OverlapSphere(attackPoint.position, attackCheckRadius, whatIsPlayer);

            for (int i = 0; i < detectedHits.Length; i++)
            {
                IDamagable damagable = detectedHits[i].GetComponent<IDamagable>();

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

    public bool IsPlayerInAggressionRange() => Vector3.Distance(transform.position, enemy.player.transform.position) < aggressionRange;

    public bool IsInBattleMode() => isInBattleMode;

    public virtual void GetHit(int damage)
    {
        enemy.health.ReduceHealth(damage);

        if (enemy.health.CheckAndMarkDeath())
        {
            enemy.dropController?.DropItems();
            enemy.Die();
        }

        EnterBattleMode();
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, aggressionRange);
    }
}
