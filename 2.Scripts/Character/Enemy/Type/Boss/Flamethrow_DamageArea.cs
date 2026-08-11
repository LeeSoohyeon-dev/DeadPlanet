using System.Collections.Generic;
using UnityEngine;

public class Flamethrow_DamageArea : MonoBehaviour
{
    private Enemy_Boss enemy;

    private float damageCooldown;
    private int flameDamage;

    private readonly Dictionary<IDamagable, float> lastTimeDamagedByTarget = new Dictionary<IDamagable, float>();

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_Boss>();

        if (enemy == null)
        {
            Debug.LogError($"{name}: 부모에서 Enemy_Boss를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        damageCooldown = enemy.flameDamageCooldown;
        flameDamage = enemy.flameDamage;
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemy.isFlamethrowActive == false)
            return;

        if ((enemy.whatIsAlly.value & (1 << other.gameObject.layer)) > 0)
            return;

        IDamagable damagable = other.GetComponent<IDamagable>();
        if (damagable == null)
            return;

        if (lastTimeDamagedByTarget.TryGetValue(damagable, out float lastTime) &&
            Time.time - lastTime < damageCooldown)
            return;

        damagable.TakeDamage(flameDamage);
        lastTimeDamagedByTarget[damagable] = Time.time;
    }

    private void OnDisable() => lastTimeDamagedByTarget.Clear();
}
