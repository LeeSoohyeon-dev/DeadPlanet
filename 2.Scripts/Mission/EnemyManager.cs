using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    
    [Header("Enemy Settings")]
    [SerializeField] private int initialEnemyCount = 10;
    [SerializeField] private bool useManualCount = true;
    
    public event Action OnEnemyKilled;
    
    public event Action<int, int> OnEnemyCountChanged;
    
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
            Debug.LogWarning("EnemyCounter 인스턴스가 이미 존재합니다. 중복 인스턴스를 제거합니다.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeEnemyCount();
        Enemy.OnAnyEnemyDied += HandleEnemyDied;

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
        
        
        OnEnemyCountChanged?.Invoke(currentEnemyCount, killedEnemyCount);
        
        return count;
    }

    public void RegisterEnemyKill()
    {
        killedEnemyCount++;
        currentEnemyCount = Mathf.Max(0, totalEnemyCount - killedEnemyCount);
        
        OnEnemyKilled?.Invoke();
        OnEnemyCountChanged?.Invoke(currentEnemyCount, killedEnemyCount);
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
        
        
        OnEnemyCountChanged?.Invoke(currentEnemyCount, killedEnemyCount);
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
        Enemy.OnAnyEnemyDied -= HandleEnemyDied;
        if (Instance == this)
        {
            Instance = null;
        }
    }
}