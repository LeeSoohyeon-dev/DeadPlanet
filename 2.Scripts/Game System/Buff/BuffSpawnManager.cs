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
	private List<Pickup_Buff> spawnedBuffs = new List<Pickup_Buff>();
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
			// 같은 GameObject의 다른 매니저를 함께 파괴하지 않도록 컴포넌트만 제거
			Destroy(this);
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
		while (waitCount < 20)
		{
			EnsurePlayerReference();

			if (player != null && player.weaponController != null)
			{
				var weaponSlots = player.weaponController.GetWeaponSlots();
				if (weaponSlots != null && weaponSlots.Count > 0)
					break;
			}

			await UniTask.Delay(50, cancellationToken: ct);
			waitCount++;
		}

		if (player == null)
			Debug.LogWarning("[BuffSpawnManager] Player를 찾지 못한 채 초기화를 시작합니다. 버프 스폰이 실패할 수 있습니다.");

		SpawnInitialBuffs();
		StartPeriodicSpawning(ct).Forget();
	}

	private void EnsurePlayerReference()
	{
		if (player == null)
			player = GameManager.GetPlayer();
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

	// 격자 탐색용 8방향 오프셋 (격자 정렬을 위해 대각선도 정규화하지 않음)
	private static readonly Vector3[] gridDirections = new Vector3[]
	{
		new Vector3(0, 0, 1),
		new Vector3(0, 0, -1),
		new Vector3(-1, 0, 0),
		new Vector3(1, 0, 0),
		new Vector3(1, 0, 1),
		new Vector3(-1, 0, 1),
		new Vector3(1, 0, -1),
		new Vector3(-1, 0, -1)
	};

	private Vector3 FindValidSpawnPositionAroundPlayer()
	{
		if (player == null)
			return Vector3.zero;

		Vector3 playerPos = player.transform.position;
		Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);

		Queue<Vector3> positionsToCheck = new Queue<Vector3>();
		HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();

		Vector3 startPos = new Vector3(playerPos.x, 0f, playerPos.z);
		positionsToCheck.Enqueue(startPos);
		visitedCells.Add(CellOf(startPos));

		int iterations = 0;

		while (positionsToCheck.Count > 0 && iterations < maxSearchIterations)
		{
			iterations++;
			Vector3 currentPos = positionsToCheck.Dequeue();

			float distanceFromPlayer = Vector2.Distance(playerPos2D, new Vector2(currentPos.x, currentPos.z));

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
				foreach (Vector3 dir in gridDirections)
				{
					Vector3 neighborPos = currentPos + dir * searchStepSize;

					if (Vector2.Distance(playerPos2D, new Vector2(neighborPos.x, neighborPos.z)) > maxDistanceFromPlayer)
						continue;

					if (visitedCells.Add(CellOf(neighborPos)))
					{
						positionsToCheck.Enqueue(neighborPos);
					}
				}
			}
		}

		return GetRandomPositionAroundPlayer();
	}

	private Vector2Int CellOf(Vector3 pos)
	{
		return new Vector2Int(
			Mathf.RoundToInt(pos.x / searchStepSize),
			Mathf.RoundToInt(pos.z / searchStepSize));
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

			// 래그돌 시체가 스폰 위치를 점유하지 않도록 제외
			if (enemy.health != null && enemy.health.IsDead)
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
		spawnedBuffs.RemoveAll(buff => buff == null || !buff.gameObject.activeInHierarchy);

		Vector2 position2D = new Vector2(position.x, position.z);

		foreach (Pickup_Buff buff in spawnedBuffs)
		{
			Vector3 buffPos = buff.transform.position;
			if (Vector2.Distance(position2D, new Vector2(buffPos.x, buffPos.z)) < minDistanceBetweenBuffs)
				return true;
		}

		return false;
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

		Pickup_Buff pickupBuff = buffObject.GetComponent<Pickup_Buff>();
		if (pickupBuff != null)
		{
			spawnedBuffs.Add(pickupBuff);
		}
	}

	public void OnBuffCollected(Pickup_Buff buff)
	{
		spawnedBuffs.Remove(buff);
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
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
		foreach (Pickup_Buff buff in spawnedBuffs)
		{
			if (buff != null)
				Gizmos.DrawWireSphere(buff.transform.position, 1f);
		}
	}
}
