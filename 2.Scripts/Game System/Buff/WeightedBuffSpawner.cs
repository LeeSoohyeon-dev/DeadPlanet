using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffSpawnEntry
{
	public GameObject buffPrefab;
	public float baseWeight = 1f;

	[System.NonSerialized] public BuffData cachedBuffData;
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

	private readonly Dictionary<GameObject, float> weightsBuffer = new Dictionary<GameObject, float>();

	// 같은 프레임에 BuffSpawnManager가 캐시를 소비할 수 있으므로 Start가 아닌 Awake에서 캐싱
	private void Awake()
	{
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
					else
					{
						entry.cachedBuffData = pickup.BuffData;
					}
				}
			}
		}
	}

	private void Start()
	{
		EnsurePlayerReference();
	}

	private void EnsurePlayerReference()
	{
		if (player == null)
			player = GameManager.GetPlayer();
	}

	public GameObject SelectRandomBuffPrefab()
	{
		if (buffPool.Count == 0)
		{
			return null;
		}

		EnsurePlayerReference();
		Dictionary<GameObject, float> weights = CalculateWeights();

		float totalWeight = 0f;
		foreach (float weight in weights.Values)
		{
			totalWeight += weight;
		}

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
		weightsBuffer.Clear();

		foreach (var entry in buffPool)
		{
			if (entry.buffPrefab == null || entry.cachedBuffData == null)
				continue;

			float finalWeight = entry.baseWeight;

			if (player != null)
			{
				float bonus = GetDynamicWeightBonus(entry.cachedBuffData.buffType);
				finalWeight += bonus;
			}

			weightsBuffer[entry.buffPrefab] = Mathf.Max(0f, finalWeight);
		}

		return weightsBuffer;
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
		if (player == null || player.weaponController == null)
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

	public GameObject GetPrefabByBuffType(BuffType buffType)
	{
		foreach (var entry in buffPool)
		{
			if (entry.buffPrefab != null && entry.cachedBuffData != null && entry.cachedBuffData.buffType == buffType)
			{
				return entry.buffPrefab;
			}
		}

		return null;
	}
}
