using System;
using System.Collections;
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
        GameEvents.OnPlaySound?.Invoke(SoundType.EnemyGrenadeImpact);

        HashSet<GameObject> uniqueEntities = new HashSet<GameObject>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider hit in colliders)
        {
            IDamagable damagable = hit.GetComponent<IDamagable>();

            if (damagable != null)
            {
                if (IsTargetValid(hit) == false)
                    continue;

                GameObject rootEntity = hit.transform.root.gameObject;
                if (uniqueEntities.Add(rootEntity) == false)
                    continue;

                damagable.TakeDamage(grenadeDamage);
            }

            ApplyPhysicalForceTo(hit);
        }
    }

    private void ApplyPhysicalForceTo(Collider hit)
    {
        Rigidbody rb = hit.GetComponent<Rigidbody>();

        if (rb != null)
            rb.AddExplosionForce(impactPower, transform.position, impactRadius, upwardsMultiplier, ForceMode.Impulse);
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
        rb.linearVelocity = CalculateLaunchVelocity(target, timeToTarget, launchPosition);
        timer = countdown + timeToTarget;
        this.impactPower = impactPower;
    }

    private bool IsTargetValid(Collider collider)
    {
        if (GameManager.instance.isFriendlyFire)
            return true;

        if((allyLayerMask.value & (1 << collider.gameObject.layer)) > 0)
            return false;

        return true;
    }

private Vector3 CalculateLaunchVelocity(Vector3 target, float timeToTarget, Vector3 launchPosition)
{
    Vector3 direction = target - launchPosition;
    Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
    float horizontalDistance = directionXZ.magnitude;
    float verticalDistance = direction.y;

    float gravity = Mathf.Abs(Physics.gravity.y);

    Vector3 horizontalDirection = directionXZ.normalized;

    float maxHeightRatio = 0.03f;
    float desiredMaxHeight = horizontalDistance * maxHeightRatio + Mathf.Max(0, verticalDistance * 0.5f);

    float timeToPeak = Mathf.Sqrt(2f * desiredMaxHeight / gravity);

    if (timeToPeak * 2f > timeToTarget)
    {
        timeToTarget = timeToPeak * 2f;
    }

    float velocityY = Mathf.Sqrt(2f * gravity * desiredMaxHeight);
    float velocityXZ = horizontalDistance / timeToTarget;

    float actualHeightAtTarget = velocityY * timeToTarget - 0.5f * gravity * timeToTarget * timeToTarget;
    float heightError = verticalDistance - actualHeightAtTarget;

    if (Mathf.Abs(heightError) > 0.01f)
    {
        velocityY += heightError / timeToTarget;
    }

    Vector3 launchVelocity = horizontalDirection * velocityXZ + Vector3.up * velocityY;

    return launchVelocity;
}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
