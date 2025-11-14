using UnityEngine;
using VInspector;

public class Player_Health : HealthController
{
    private Player player;

    public bool isDead { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public override void ReduceHealth(int damage)
    {
        int processedDamage = damage;

        if (BuffManager.instance != null)
        {
            processedDamage = BuffManager.instance.ProcessDamageWithShield(damage);
        }

        base.ReduceHealth(processedDamage);

        if (CheckAndMarkDeath())
            Die();

        GameEvents.OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public override void RestoreHealth(int amount)
    {
        base.RestoreHealth(amount);
        GameEvents.OnPlayerHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        player.weaponVisuals.ReduceRigWeight();
        player.weaponVisuals.SwitchAnimationLayer(0);
        player.anim.SetTrigger("Die");

        GameEvents.OnGameOver?.Invoke();

    }
}
