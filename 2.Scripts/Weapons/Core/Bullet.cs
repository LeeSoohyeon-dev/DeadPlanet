using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int bulletDamage;
    private float impactForce;

    private BoxCollider cd;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;

    [SerializeField] private GameObject bulletImpactFX;

    private Vector3 startPosition;
    private float flyDistance;
    private float flyDistanceSqr;
    private float fadeThresholdSqr;
    private bool isBulletDisabled;

    private LayerMask allyLayerMask;

    protected virtual void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void BulletSetup(LayerMask allyLayerMask, int bulletDamage, float flyDistance = 100, float impactForce = 100)
    {
        this.allyLayerMask = allyLayerMask;
        this.impactForce = impactForce;
        this.bulletDamage = bulletDamage;

        isBulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;

        trailRenderer.Clear();
        trailRenderer.time = .25f;
        startPosition = transform.position;
        this.flyDistance = flyDistance + .5f;

        flyDistanceSqr = this.flyDistance * this.flyDistance;
        float fadeThreshold = Mathf.Max(this.flyDistance - 1.5f, 0f);
        fadeThresholdSqr = fadeThreshold * fadeThreshold;
    }

    public void SetVelocity(Vector3 direction, float speed, float referenceBulletSpeed)
    {
        // 발사 속도가 달라도 충돌 시 운동량이 일정하도록 질량으로 보정
        rb.mass = referenceBulletSpeed / speed;
        rb.linearVelocity = direction * speed;
    }

    protected virtual void Update()
    {
        float travelledSqr = (transform.position - startPosition).sqrMagnitude;

        FadeTrailIfNeeded(travelledSqr);
        DisableBulletIfNeeded(travelledSqr);
        ReturnToPoolIfNeeded();
    }

    protected void ReturnToPoolIfNeeded()
    {
        if (trailRenderer.time < 0)
            ReturnBulletToPool();
    }
    protected void DisableBulletIfNeeded(float travelledSqr)
    {
        if (travelledSqr > flyDistanceSqr && !isBulletDisabled)
            DisableBullet();
    }
    protected void FadeTrailIfNeeded(float travelledSqr)
    {
        if (travelledSqr > fadeThresholdSqr)
            trailRenderer.time -= 2 * Time.deltaTime;
    }

    private void DisableBullet()
    {
        isBulletDisabled = true;
        cd.enabled = false;
        meshRenderer.enabled = false;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isBulletDisabled)
            return;

        DisableBullet();

        if (IsFriendlyFire() == false)
        {
            if ((allyLayerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                ReturnBulletToPool();
                return;
            }
        }

        CreateImpactFx();
        ReturnBulletToPool();

        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
        damagable?.TakeDamage(bulletDamage);

        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            GameEvents.RaisePlaySound(SoundType.EnemyHurt);
            ApplyBulletImpactToEnemy(collision, enemy);
        }
        else if (collision.gameObject.GetComponentInParent<Player>() != null)
        {
            GameEvents.RaisePlaySound(SoundType.PlayerBulletHurt);
        }
    }

    private void ApplyBulletImpactToEnemy(Collision collision, Enemy enemy)
    {
        Vector3 force = rb.linearVelocity.normalized * impactForce;
        Rigidbody hitRigidbody = collision.collider.attachedRigidbody;
        enemy.movement.BulletImpact(force, collision.contacts[0].point, hitRigidbody);
    }

    protected void ReturnBulletToPool(float delay = 0) => ObjectPool.instance.ReturnObject(gameObject, delay);

    protected void CreateImpactFx()
    {
        GameObject newImpactFx = ObjectPool.instance.GetObject(bulletImpactFX, transform);
        ObjectPool.instance.ReturnObject(newImpactFx, 1);
    }

    private bool IsFriendlyFire() => GameManager.instance != null && GameManager.instance.isFriendlyFire;
}
