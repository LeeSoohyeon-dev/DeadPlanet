using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    private bool isDead;

    // 상태 조회 전용 (CheckAndMarkDeath는 사망 전이를 1회만 반환하는 소비형)
    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void ReduceHealth(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;
    }

    public virtual void RestoreHealth(int amount)
    {
        // 사망 후 회복은 조작 불가 상태에서 체력바만 채우게 되므로 무시
        if (isDead)
            return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public bool CheckAndMarkDeath()
    {
        if (isDead) return false;

        if (currentHealth <= 0)
        {
            isDead = true;
            return true;
        }
        return false;
    }
}
