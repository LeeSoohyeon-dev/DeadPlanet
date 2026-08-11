using UnityEngine;

public class Player_Health : HealthController
{
    private Player player;

    public bool isDead => IsDead;

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        GameEvents.RaisePlayerHealthChanged(currentHealth, maxHealth);
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

        GameEvents.RaisePlayerHealthChanged(currentHealth, maxHealth);
    }

    public override void RestoreHealth(int amount)
    {
        base.RestoreHealth(amount);
        GameEvents.RaisePlayerHealthChanged(currentHealth, maxHealth);
    }
    private void Die()
    {
        player.weaponVisuals.ReduceRigWeight();
        player.weaponVisuals.SwitchAnimationLayer(0);
        player.anim.SetTrigger("Die");

        GameEvents.RaiseGameOver();

    }
}
