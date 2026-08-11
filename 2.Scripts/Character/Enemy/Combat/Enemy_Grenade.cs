using System.Collections.Generic;
using UnityEngine;

public class Enemy_Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionFx;
    [SerializeField] private float impactRadius;
    [SerializeField] private float upwardsMultiplier = 1;
    private Rigidbody rb;
    private float timer;
    private float impactPower;

    private LayerMask allyLayerMask;
    private bool canExplode = true;

    private int grenadeDamage;

    // 근거리 투척 시 발사 속도 발산을 막는 비행시간 하한
    private const float MIN_TIME_TO_TARGET = 0.5f;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0 && canExplode)
            Explode();
    }

    private void Explode()
    {
        canExplode = false;
        PlayExplosionFx();
        GameEvents.RaisePlaySound(SoundType.EnemyGrenadeImpact);

        HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider hit in colliders)
        {
            if (IsTargetValid(hit) == false)
                continue;

            IDamagable damagable = hit.GetComponent<IDamagable>();

            if (damagable != null)
            {
                GameObject rootEntity = hit.transform.root.gameObject;
                if (uniqueEntities.Add(rootEntity))
                    damagable.TakeDamage(grenadeDamage);
            }

            ApplyPhysicalForceTo(hit);
        }
    }

    private void ApplyPhysicalForceTo(Collider hit)
    {
        Rigidbody hitRb = hit.GetComponent<Rigidbody>();

        if (hitRb != null)
            hitRb.AddExplosionForce(impactPower, transform.position, impactRadius, upwardsMultiplier, ForceMode.Impulse);
    }

    private void PlayExplosionFx()
    {
        GameObject newFx = ObjectPool.instance.GetObject(explosionFx, transform);
        ObjectPool.instance.ReturnObject(newFx, 1);
        ObjectPool.instance.ReturnObject(gameObject);
    }

    public void SetupGrenade(LayerMask allyLayerMask, Vector3 target, float timeToTarget, float countdown, float impactPower, int grenadeDamage)
    {
        SetupGrenade(allyLayerMask, target, timeToTarget, countdown, impactPower, grenadeDamage, transform.position);
    }

    public void SetupGrenade(LayerMask allyLayerMask, Vector3 target, float timeToTarget, float countdown, float impactPower, int grenadeDamage, Vector3 launchPosition)
    {
        canExplode = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        this.grenadeDamage = grenadeDamage;
        this.allyLayerMask = allyLayerMask;

        timeToTarget = Mathf.Max(timeToTarget, MIN_TIME_TO_TARGET);

        rb.linearVelocity = CalculateLaunchVelocity(target, timeToTarget, launchPosition);
        timer = countdown + timeToTarget;
        this.impactPower = impactPower;
    }

    private bool IsTargetValid(Collider collider)
    {
        if (GameManager.instance != null && GameManager.instance.isFriendlyFire)
            return true;

        if ((allyLayerMask.value & (1 << collider.gameObject.layer)) > 0)
            return false;

        return true;
    }

    // 목표 지점과 비행시간이 주어지면 포물선 발사 속도는 닫힌 해로 구해진다:
    //   v_xz = d / t,  v_y = (Δy + ½·g·t²) / t
    private Vector3 CalculateLaunchVelocity(Vector3 target, float timeToTarget, Vector3 launchPosition)
    {
        Vector3 direction = target - launchPosition;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
        float horizontalDistance = directionXZ.magnitude;
        float verticalDistance = direction.y;

        float gravity = Mathf.Abs(Physics.gravity.y);

        float velocityXZ = horizontalDistance / timeToTarget;
        float velocityY = (verticalDistance + 0.5f * gravity * timeToTarget * timeToTarget) / timeToTarget;

        Vector3 horizontalDirection = horizontalDistance > 0.001f ? directionXZ / horizontalDistance : Vector3.zero;

        return horizontalDirection * velocityXZ + Vector3.up * velocityY;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
