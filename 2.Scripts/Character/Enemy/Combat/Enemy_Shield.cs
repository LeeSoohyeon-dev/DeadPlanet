using UnityEngine;

public class Enemy_Shield : MonoBehaviour, IDamagable
{
    private Enemy_Melee enemy;
    [SerializeField] private int durability;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_Melee>();
        durability = enemy.shieldDurability;
    }

    public void ReduceDurability(int damage)
    {
        durability -= damage;

        if (durability <= 0)
        {
            enemy.anim.SetFloat("ChaseIndex", 0);
            gameObject.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        ReduceDurability(damage);
    }
}
