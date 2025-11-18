using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class BuffSpawnEntry
{
	public GameObject buffPrefab;
	public float baseWeight = 1f;
}

public class WeightedBuffSpawner : MonoBehaviour
{
	[Header("Buff Pool")]
	[SerializeField] private List<BuffSpawnEntry> buffPool = new List<BuffSpawnEntry>();

	[Header("Dynamic Weight Settings")]
	[SerializeField] private float lowHealthThreshold = 0.3f;
	[SerializeField] private float lowHealthWeightBonus = 0.4f;

	[SerializeField] private int ammoLowThreshold = 80;
	[SerializeField] private float lowAmmoWeightBonus = 0.3f;

	private Player player;

	private void Start()
	{
		EnsurePlayerReference();

		if (buffPool.Count == 0)
		{
			Debug.LogError("[WeightedBuffSpawner] Buff Pool이 비어있습니다!");
		}
		else
		{
			foreach (var entry in buffPool)
			{
				if (entry.buffPrefab == null)
				{
					Debug.LogError("[WeightedBuffSpawner] Buff Pool에 null 프리팹이 있습니다!");
				}
				else
				{
					Pickup_Buff pickup = entry.buffPrefab.GetComponent<Pickup_Buff>();
					if (pickup == null)
					{
						Debug.LogError($"[WeightedBuffSpawner] {entry.buffPrefab.name} 프리팹에 Pickup_Buff 컴포넌트가 없습니다!");
					}
					else if (pickup.BuffData == null)
					{
						Debug.LogError($"[WeightedBuffSpawner] {entry.buffPrefab.name} 프리팹에 BuffData가 할당되지 않았습니다!");
					}
				}
			}
		}
	}

	private void EnsurePlayerReference()
	{
		if (player != null)
			return;

		if (GameManager.instance != null && GameManager.instance.player != null)
		{
			player = GameManager.instance.player;
			return;
		}

		player = FindFirstObjectByType<Player>();
		if (player != null && GameManager.instance != null)
		{
			GameManager.instance.player = player;
		}
	}

	public GameObject SelectRandomBuffPrefab()
	{
		if (buffPool.Count == 0)
		{
			return null;
		}

		EnsurePlayerReference();
		Dictionary<GameObject, float> weights = CalculateWeights();

		float totalWeight = weights.Values.Sum();
		if (totalWeight <= 0f)
		{
			return buffPool[0].buffPrefab;
		}

		float randomValue = Random.Range(0f, totalWeight);
		float cumulativeWeight = 0f;

		foreach (var entry in weights)
		{
			cumulativeWeight += entry.Value;

			if (randomValue <= cumulativeWeight)
			{
			return entry.Key;
			}
		}

		return buffPool[0].buffPrefab;
	}

	private Dictionary<GameObject, float> CalculateWeights()
	{
		Dictionary<GameObject, float> weights = new Dictionary<GameObject, float>();

		bool isHealthLow = false;
		bool isAmmoLow = false;

		try
		{
			isHealthLow = IsPlayerHealthLow();
			isAmmoLow = IsPlayerAmmoLow();
		}
		catch (System.Exception e)
		{
			Debug.LogWarning($"[WeightedBuffSpawner] 플레이어 상태 체크 중 오류 발생: {e.Message}");
		}

		foreach (var entry in buffPool)
		{
			if (entry.buffPrefab == null)
				continue;

			Pickup_Buff pickup = entry.buffPrefab.GetComponent<Pickup_Buff>();
			if (pickup == null)
				continue;

			BuffData buffData = pickup.BuffData;
			if (buffData == null)
			{
				continue;
			}

			float finalWeight = entry.baseWeight;

			if (player != null)
			{
				float bonus = GetDynamicWeightBonus(buffData.buffType);
				finalWeight += bonus;
			}

			weights[entry.buffPrefab] = Mathf.Max(0f, finalWeight);
		}

		return weights;
	}

	private float GetDynamicWeightBonus(BuffType buffType)
	{
		float bonus = 0f;

		switch (buffType)
		{
			case BuffType.HealthRestore:
				if (IsPlayerHealthLow())
				{
					bonus += lowHealthWeightBonus;
				}
				break;

			case BuffType.AmmoRestore:
				if (IsPlayerAmmoLow())
				{
					bonus += lowAmmoWeightBonus;
				}
				break;
		}

		return bonus;
	}

	private bool IsPlayerHealthLow()
	{
		if (player == null || player.health == null)
			return false;

		float healthRatio = (float)player.health.currentHealth / player.health.maxHealth;
		return healthRatio < lowHealthThreshold;
	}

	private bool IsPlayerAmmoLow()
	{
		try
		{
			if (player == null)
				return false;

			if (player.weaponController == null)
				return false;

			List<Weapon> weaponSlots = player.weaponController.GetWeaponSlots();
			if (weaponSlots == null || weaponSlots.Count == 0)
				return false;

			foreach (Weapon weapon in weaponSlots)
			{
				if (weapon == null)
					continue;

				if (weapon.totalReserveAmmo <= ammoLowThreshold)
					return true;
			}

			return false;
		}
		catch (System.NullReferenceException)
		{
			return false;
		}
		catch (System.Exception)
		{
			return false;
		}
	}

	public void AddBuffToPool(GameObject buffPrefab, float weight = 1f)
	{
		if (buffPrefab == null)
			return;

		BuffSpawnEntry entry = new BuffSpawnEntry
		{
			buffPrefab = buffPrefab,
			baseWeight = weight
		};

		buffPool.Add(entry);
	}

	public List<BuffSpawnEntry> GetBuffPool() => buffPool;

	public GameObject GetPrefabByBuffType(BuffType buffType)
	{
		foreach (var entry in buffPool)
		{
			if (entry.buffPrefab == null)
				continue;

			Pickup_Buff pickup = entry.buffPrefab.GetComponent<Pickup_Buff>();
			if (pickup != null && pickup.BuffData != null && pickup.BuffData.buffType == buffType)
			{
				return entry.buffPrefab;
			}
		}

		return null;
	}
}
