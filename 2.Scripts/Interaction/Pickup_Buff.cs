using UnityEngine;

public class Pickup_Buff : MonoBehaviour
{
	[Header("Buff Settings")]
	[SerializeField] private BuffData buffData;

	public BuffData BuffData => buffData;

	[Header("Pickup Settings")]
	[SerializeField] private float autoPickupRadius = 2f;
	[SerializeField] private LayerMask playerLayer;

	private bool isCollected = false;

	private void OnEnable()
	{
		isCollected = false;
	}

	private void OnDisable()
	{
		isCollected = false;
	}

	private void Update()
	{
		if (isCollected)
			return;

		CheckAutoPickup();
	}

	private void CheckAutoPickup()
	{
		if (buffData == null)
		{
			return;
		}

		Collider[] hits = Physics.OverlapSphere(transform.position, autoPickupRadius, playerLayer);

		if (hits.Length > 0)
		{
			CollectBuff();
		}
	}

	private void CollectBuff()
	{
		if (isCollected)
			return;

		isCollected = true;

		if (BuffManager.instance != null)
		{
			GameObject particlePrefab = BuffManager.instance.GetCollectParticlePrefab();
			if (particlePrefab != null)
			{
				GameObject particle = ObjectPool.instance.GetObject(particlePrefab, transform);
				if (particle != null)
				{
					particle.transform.position = transform.position;

					ParticleSystem ps = particle.GetComponent<ParticleSystem>();
					if (ps != null)
					{
						float duration = ps.main.duration + ps.main.startLifetime.constantMax;
						ObjectPool.instance.ReturnObject(particle, duration);
					}
					else
					{
						ObjectPool.instance.ReturnObject(particle, 2f);
					}
				}
			}
		}

		if (BuffManager.instance != null && buffData != null)
		{
			BuffManager.instance.ApplyBuff(buffData);
		}

		if (BuffSpawnManager.instance != null)
		{
			BuffSpawnManager.instance.OnBuffCollected(transform.position);
		}

		if (GetComponent<PooledObject>() != null)
		{
			ObjectPool.instance.ReturnObject(gameObject);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	public void SetBuffData(BuffData data)
	{
		buffData = data;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, autoPickupRadius);
	}
}
