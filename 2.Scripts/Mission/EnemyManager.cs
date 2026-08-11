using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Enemy Settings")]
    [SerializeField] private int initialEnemyCount = 10;
    [SerializeField] private bool useManualCount = true;

    private int totalEnemyCount = 0;
    private int killedEnemyCount = 0;
    private int currentEnemyCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // 같은 GameObject에 MissionManager가 함께 있어 gameObject를 파괴하면 안 됨
            Destroy(this);
            return;
        }

        // WaveManager.Start의 스폰 등록이 초기값에 덮이지 않도록 Awake에서 초기화
        InitializeEnemyCount();
    }

    private void OnEnable()
    {
        if (Instance != this)
            return;

        GameEvents.OnAnyEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        GameEvents.OnAnyEnemyDied -= HandleEnemyDied;
    }

    public int CountEnemiesInScene(LayerMask enemyLayer)
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Enemy enemy in enemies)
        {
            if (IsInLayerMask(enemy.gameObject, enemyLayer))
            {
                count++;
            }
        }

        totalEnemyCount = count;
        currentEnemyCount = count;
        killedEnemyCount = 0;

        return count;
    }

    public void RegisterEnemyKill()
    {
        killedEnemyCount++;
        currentEnemyCount = Mathf.Max(0, totalEnemyCount - killedEnemyCount);
    }

    // 런타임에 스폰된 적을 전체 적 수에 반영
    public void RegisterSpawnedEnemy()
    {
        totalEnemyCount++;
        currentEnemyCount = Mathf.Max(0, totalEnemyCount - killedEnemyCount);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        RegisterEnemyKill();
    }

    public void SetEnemyCount(int enemyCount)
    {
        initialEnemyCount = enemyCount;
        totalEnemyCount = enemyCount;
        currentEnemyCount = enemyCount;
        killedEnemyCount = 0;
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }

    private void InitializeEnemyCount()
    {
        if (useManualCount)
        {
            SetEnemyCount(initialEnemyCount);
        }
        else
        {
            LayerMask defaultEnemyLayer = LayerMask.GetMask("Enemy");
            CountEnemiesInScene(defaultEnemyLayer);
        }
    }

    public int TotalEnemyCount => totalEnemyCount;

    public int KilledEnemyCount => killedEnemyCount;

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
