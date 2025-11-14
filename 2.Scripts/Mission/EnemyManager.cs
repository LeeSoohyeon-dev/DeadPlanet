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
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeEnemyCount();
    }

    private void OnEnable()
    {
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}