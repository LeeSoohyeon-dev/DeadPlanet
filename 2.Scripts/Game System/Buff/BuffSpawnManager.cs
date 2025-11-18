using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BuffSpawnManager : MonoBehaviour
{
	public static BuffSpawnManager instance;

	[Header("Spawn Settings")]
	[SerializeField] private int initialBuffCount = 3;
	[SerializeField] private float spawnInterval = 10f;
	[SerializeField] private float minDistanceFromPlayer = 5f;
	[SerializeField] private float maxDistanceFromPlayer = 15f;
	[SerializeField] private float minDistanceFromEnemies = 8f;
	[SerializeField] private float minDistanceBetweenBuffs = 5f;

	[Header("BFS Settings")]
	[SerializeField] private float searchStepSize = 3f;
	[SerializeField] private int maxSearchIterations = 200;

	private WeightedBuffSpawner buffSpawner;
	private List<Vector3> spawnedBuffPositions = new List<Vector3>();
	private Player player;
	private List<Enemy> cachedEnemies = new List<Enemy>();
	private float enemyCacheUpdateInterval = 1f;
	private float lastEnemyCacheUpdate = 0f;

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

		buffSpawner = GetComponent<WeightedBuffSpawner>();
		if (buffSpawner == null)
		{
			Debug.LogError("[BuffSpawnManager] WeightedBuffSpawner 컴포넌트가 없습니다. 같은 GameObject에 추가해주세요.");
		}
	}

	private void Start()
	{
		EnsurePlayerReference();

		if (buffSpawner == null)
		{
			Debug.LogError("[BuffSpawnManager] WeightedBuffSpawner가 없습니다. 같은 GameObject에 WeightedBuffSpawner 컴포넌트를 추가해주세요.");
			return;
		}

		StartInitialization(this.GetCancellationTokenOnDestroy()).Forget();
	}

	private async UniTask StartInitialization(CancellationToken ct)
	{
		int waitCount = 0;
		while (player != null && player.weaponController != null && waitCount < 20)
		{
			try
			{
				var weaponSlots = player.weaponController.GetWeaponSlots();
				if (weaponSlots != null && weaponSlots.Count > 0)
				{
					break;
				}
			}
			catch
			{
			}

			await UniTask.Delay(50, cancellationToken: ct);
			waitCount++;
		}

		SpawnInitialBuffs();
		StartPeriodicSpawning(ct).Forget();
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

	private void SpawnInitialBuffs()
	{
		for (int i = 0; i < initialBuffCount; i++)
		{
			Vector3 spawnPosition = FindValidSpawnPositionAroundPlayer();

			if (spawnPosition != Vector3.zero)
			{
				Vector3 spawnPos2D = new Vector3(spawnPosition.x, 0f, spawnPosition.z);
				if (IsValidSpawnPosition(spawnPos2D))
				{
					SpawnBuff(spawnPosition, BuffType.AmmoRestore);
					spawnedBuffPositions.Add(spawnPos2D);
				}
			}
		}
	}

	private async UniTask StartPeriodicSpawning(CancellationToken ct)
	{
		try
		{
			await UniTask.WaitForSeconds(spawnInterval, cancellationToken: ct);

			while (!ct.IsCancellationRequested)
			{
				Vector3 spawnPosition = FindValidSpawnPositionAroundPlayer();

				if (spawnPosition != Vector3.zero)
				{
					Vector3 spawnPos2D = new Vector3(spawnPosition.x, 0f, spawnPosition.z);
					if (IsValidSpawnPosition(spawnPos2D))
					{
						SpawnBuff(spawnPosition);
						spawnedBuffPositions.Add(spawnPos2D);
					}
				}

				await UniTask.WaitForSeconds(spawnInterval, cancellationToken: ct);
			}
		}
		catch (System.OperationCanceledException)
		{
			Debug.Log("[BuffSpawnManager] 주기적 버프 스폰 작업 취소");
		}
	}

	private Vector3 FindValidSpawnPositionAroundPlayer()
	{
		if (player == null)
			return Vector3.zero;

		Vector3 playerPos = player.transform.position;
		Queue<Vector3> positionsToCheck = new Queue<Vector3>();
		HashSet<Vector3> visitedPositions = new HashSet<Vector3>();

		Vector3 startPos = new Vector3(playerPos.x, 0f, playerPos.z);
		positionsToCheck.Enqueue(startPos);
		visitedPositions.Add(RoundPosition(startPos));

		int iterations = 0;

		while (positionsToCheck.Count > 0 && iterations < maxSearchIterations)
		{
			iterations++;
			Vector3 currentPos = positionsToCheck.Dequeue();
			currentPos = new Vector3(currentPos.x, 0f, currentPos.z);

			Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);
			Vector2 currentPos2D = new Vector2(currentPos.x, currentPos.z);
			float distanceFromPlayer = Vector2.Distance(playerPos2D, currentPos2D);

			if (distanceFromPlayer >= minDistanceFromPlayer && distanceFromPlayer <= maxDistanceFromPlayer)
			{
				NavMeshHit hit;
				if (NavMesh.SamplePosition(currentPos, out hit, 5f, NavMesh.AllAreas))
				{
					if (IsValidSpawnPosition(currentPos))
					{
						return new Vector3(hit.position.x, hit.position.y + 0.5f, hit.position.z);
					}
				}
			}

			if (distanceFromPlayer < maxDistanceFromPlayer)
			{
				Vector3[] directions = new Vector3[]
				{
					Vector3.forward,
					Vector3.back,
					Vector3.left,
					Vector3.right,
					(Vector3.forward + Vector3.right).normalized,
					(Vector3.forward + Vector3.left).normalized,
					(Vector3.back + Vector3.right).normalized,
					(Vector3.back + Vector3.left).normalized
				};

				foreach (Vector3 dir in directions)
				{
					Vector3 neighborPos = new Vector3((currentPos + dir * searchStepSize).x, 0f, (currentPos + dir * searchStepSize).z);

					Vector2 neighborPos2D = new Vector2(neighborPos.x, neighborPos.z);
					float dist = Vector2.Distance(playerPos2D, neighborPos2D);

					if (dist > maxDistanceFromPlayer)
						continue;

					Vector3 roundedPos = RoundPosition(neighborPos);

					if (visitedPositions.Add(roundedPos))
					{
						positionsToCheck.Enqueue(neighborPos);
					}
				}
			}
		}

		return GetRandomPositionAroundPlayer();
	}

	private Vector3 GetRandomPositionAroundPlayer()
	{
		if (player == null)
			return Vector3.zero;

		Vector3 playerPos = player.transform.position;

		for (int i = 0; i < 50; i++)
		{
			float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
			float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
			Vector3 randomPos = new Vector3(playerPos.x + Mathf.Cos(angle) * distance, 0f, playerPos.z + Mathf.Sin(angle) * distance);

			NavMeshHit hit;
			if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
			{
				Vector3 spawnPos2D = new Vector3(hit.position.x, 0f, hit.position.z);
				if (IsValidSpawnPosition(spawnPos2D))
				{
					return new Vector3(hit.position.x, hit.position.y + 0.5f, hit.position.z);
				}
			}
		}

		return Vector3.zero;
	}

	private bool IsValidSpawnPosition(Vector3 position)
	{
		Vector3 checkPos = new Vector3(position.x, 0f, position.z);

		NavMeshHit hit;
		if (!NavMesh.SamplePosition(checkPos, out hit, 5f, NavMesh.AllAreas))
			return false;

		Vector3 navMeshPos = hit.position;

		if (player != null)
		{
			Vector3 playerPos = player.transform.position;
			Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);
			Vector2 navMeshPos2D = new Vector2(navMeshPos.x, navMeshPos.z);
			float distance2D = Vector2.Distance(playerPos2D, navMeshPos2D);

			if (distance2D < minDistanceFromPlayer)
				return false;
		}

		Vector3 checkPos2D = new Vector3(navMeshPos.x, 0f, navMeshPos.z);
		if (IsTooCloseToEnemies(checkPos2D))
			return false;

		if (IsTooCloseToOtherBuffs(checkPos2D))
			return false;

		return true;
	}

	private bool IsOnNavMesh(Vector3 position)
	{
		NavMeshHit hit;
		return NavMesh.SamplePosition(position, out hit, 5f, NavMesh.AllAreas);
	}

	private bool IsTooCloseToEnemies(Vector3 position)
	{
		UpdateEnemyCache();

		float checkRadius = maxDistanceFromPlayer + minDistanceFromEnemies;
		Vector2 playerPos2D = player != null ? new Vector2(player.transform.position.x, player.transform.position.z) : Vector2.zero;

		foreach (Enemy enemy in cachedEnemies)
		{
			if (enemy == null)
				continue;

			Vector2 enemyPos2D = new Vector2(enemy.transform.position.x, enemy.transform.position.z);
			
			if (player != null && Vector2.Distance(playerPos2D, enemyPos2D) > checkRadius)
				continue;

			if (Vector3.Distance(position, enemy.transform.position) < minDistanceFromEnemies)
				return true;
		}

		return false;
	}

	private void UpdateEnemyCache()
	{
		if (Time.time - lastEnemyCacheUpdate < enemyCacheUpdateInterval)
			return;

		lastEnemyCacheUpdate = Time.time;

		cachedEnemies.Clear();
		
		if (player == null)
			return;

		float cacheRadius = maxDistanceFromPlayer + minDistanceFromEnemies;
		Vector2 playerPos2D = new Vector2(player.transform.position.x, player.transform.position.z);
		
		Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
		
		foreach (Enemy enemy in enemies)
		{
			if (enemy == null)
				continue;

			Vector2 enemyPos2D = new Vector2(enemy.transform.position.x, enemy.transform.position.z);
			
			if (Vector2.Distance(playerPos2D, enemyPos2D) <= cacheRadius)
			{
				cachedEnemies.Add(enemy);
			}
		}
	}

	private bool IsTooCloseToOtherBuffs(Vector3 position)
	{
		foreach (Vector3 buffPos in spawnedBuffPositions)
		{
			if (Vector3.Distance(position, buffPos) < minDistanceBetweenBuffs)
				return true;
		}

		return false;
	}

	private Vector3 RoundPosition(Vector3 pos)
	{
		float precision = 1f;
		return new Vector3(
			Mathf.Round(pos.x / precision) * precision,
			Mathf.Round(pos.y / precision) * precision,
			Mathf.Round(pos.z / precision) * precision
		);
	}

	private void SpawnBuff(Vector3 position)
	{
		SpawnBuff(position, null);
	}

	private void SpawnBuff(Vector3 position, BuffType? forcedBuffType)
	{
		if (buffSpawner == null)
		{
			return;
		}

		GameObject prefabToSpawn = null;

		if (forcedBuffType.HasValue)
		{
			prefabToSpawn = buffSpawner.GetPrefabByBuffType(forcedBuffType.Value);
			if (prefabToSpawn == null)
			{
				prefabToSpawn = buffSpawner.SelectRandomBuffPrefab();
			}
		}
		else
		{
			prefabToSpawn = buffSpawner.SelectRandomBuffPrefab();
		}

		if (prefabToSpawn == null)
		{
			return;
		}

		if (ObjectPool.instance == null)
		{
			Debug.LogError("[BuffSpawnManager] ObjectPool.instance가 null입니다.");
			return;
		}

		GameObject buffObject = ObjectPool.instance.GetObject(prefabToSpawn, null);
		if (buffObject == null)
		{
			Debug.LogError($"[BuffSpawnManager] 버프 오브젝트 생성 실패: {prefabToSpawn.name}");
			return;
		}

		buffObject.transform.position = position;
	}

	public void OnBuffCollected(Vector3 position)
	{
		spawnedBuffPositions.Remove(position);
	}

	private void OnDrawGizmosSelected()
	{
		if (player != null)
		{
			Vector3 playerPos = player.transform.position;

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(playerPos, minDistanceFromPlayer);

			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(playerPos, maxDistanceFromPlayer);
		}

		Gizmos.color = Color.yellow;
		foreach (Vector3 pos in spawnedBuffPositions)
		{
			Gizmos.DrawWireSphere(pos, 1f);
		}
	}
}
