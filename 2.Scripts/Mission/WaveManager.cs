using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[System.Serializable]
public class WaveEnemyGroup
{
	public GameObject enemyPrefab;
	public int minCount = 1;
	public int maxCount = 3;
	public float spawnWeight = 1f;
}

public class WaveManager : MonoBehaviour
{
	public static WaveManager instance;

	[Header("Wave Settings")]
	[SerializeField] private bool useProceduralWaves = true;
	[SerializeField] private int totalWaves = 5;
	[SerializeField] private float timeBetweenWaves = 10f;

	[Header("Perlin Noise Settings")]
	[SerializeField] private float noiseFrequency = 0.5f;
	[SerializeField] private float noiseSeed = 0f;
	[SerializeField] private int minEnemiesPerWave = 3;
	[SerializeField] private int maxEnemiesPerWave = 10;

	[Header("Spawn Settings")]
	[SerializeField] private List<Transform> spawnPoints = new List<Transform>();
	[SerializeField] private List<WaveEnemyGroup> enemyGroups = new List<WaveEnemyGroup>();

	private int currentWave = 0;
	private float waveStartTime = 0f;
	private bool isWaveActive = false;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		if (noiseSeed == 0f)
		{
			noiseSeed = Random.Range(0f, 1000f);
		}
	}

	private void Start()
	{
		if (useProceduralWaves)
		{
			StartProceduralWaves();
		}
	}

	public void StartProceduralWaves()
	{
		waveStartTime = Time.time;
		StartWaveSequenceAsync(this.GetCancellationTokenOnDestroy()).Forget();
	}

	private async UniTask StartWaveSequenceAsync(CancellationToken ct)
	{
		try
		{
			for (int i = 0; i < totalWaves; i++)
			{
				currentWave = i + 1;

				int enemyCount = CalculateEnemyCountWithPerlinNoise(currentWave);
				float spawnInterval = CalculateSpawnIntervalWithPerlinNoise(currentWave);

				isWaveActive = true;
				await SpawnWaveAsync(enemyCount, spawnInterval, ct);
				isWaveActive = false;

				if (i < totalWaves - 1)
				{
					await UniTask.WaitForSeconds(timeBetweenWaves, cancellationToken: ct);
				}
			}
		}
		catch (System.OperationCanceledException)
		{
			Debug.Log("[WaveManager] 웨이브 시퀀스 취소");
		}
	}

	private int CalculateEnemyCountWithPerlinNoise(int wave)
	{
		float time = wave * noiseFrequency;
		float noiseValue = Mathf.PerlinNoise(time, noiseSeed);

		int adjustedMin = minEnemiesPerWave + (wave - 1);
		int adjustedMax = maxEnemiesPerWave + (wave - 1);

		int enemyCount = Mathf.RoundToInt(Mathf.Lerp(adjustedMin, adjustedMax, noiseValue));

		return enemyCount;
	}

	private float CalculateSpawnIntervalWithPerlinNoise(int wave)
	{
		float time = wave * noiseFrequency + 100f;
		float noiseValue = Mathf.PerlinNoise(time, noiseSeed);

		float minInterval = 0.5f;
		float maxInterval = 3f;

		return Mathf.Lerp(minInterval, maxInterval, noiseValue);
	}

	private async UniTask SpawnWaveAsync(int enemyCount, float spawnInterval, CancellationToken ct)
	{
		for (int i = 0; i < enemyCount; i++)
		{
			SpawnRandomEnemy();

			if (i < enemyCount - 1)
			{
				await UniTask.WaitForSeconds(spawnInterval, cancellationToken: ct);
			}
		}
	}

	private void SpawnRandomEnemy()
	{
		if (spawnPoints.Count == 0 || enemyGroups.Count == 0)
		{
			return;
		}

		Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
		WaveEnemyGroup selectedGroup = SelectEnemyGroupByWeight();

		if (selectedGroup != null && selectedGroup.enemyPrefab != null)
		{
			int count = Random.Range(selectedGroup.minCount, selectedGroup.maxCount + 1);

			for (int i = 0; i < count; i++)
			{
				Vector3 spawnOffset = Random.insideUnitSphere * 2f;
				spawnOffset.y = 0;
				Vector3 spawnPos = spawnPoint.position + spawnOffset;

				Instantiate(selectedGroup.enemyPrefab, spawnPos, Quaternion.identity);
			}
		}
	}

	private WaveEnemyGroup SelectEnemyGroupByWeight()
	{
		float totalWeight = 0f;
		foreach (var group in enemyGroups)
		{
			totalWeight += group.spawnWeight;
		}

		float randomValue = Random.Range(0f, totalWeight);
		float cumulativeWeight = 0f;

		foreach (var group in enemyGroups)
		{
			cumulativeWeight += group.spawnWeight;

			if (randomValue <= cumulativeWeight)
			{
				return group;
			}
		}

		return enemyGroups[0];
	}

	public int GetCurrentWave() => currentWave;

	public bool IsWaveActive() => isWaveActive;

	public float GetNoiseValue(float time)
	{
		return Mathf.PerlinNoise(time * noiseFrequency, noiseSeed);
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
			return;

		Gizmos.color = Color.green;
		foreach (Transform point in spawnPoints)
		{
			if (point != null)
			{
				Gizmos.DrawWireSphere(point.position, 1f);
			}
		}
	}
}
