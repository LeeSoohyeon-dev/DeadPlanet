using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] private int poolSize = 10;
    [SerializeField] private int maxPoolSize = 100;

    private Dictionary<GameObject, IObjectPool<GameObject>> poolDictionary =
        new Dictionary<GameObject, IObjectPool<GameObject>>();

    [SerializeField] private List<GameObject> weaponPickup = new List<GameObject>();
    [SerializeField] private List<GameObject> buffPickups = new List<GameObject>();
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
    }

    private void Start()
    {
        for (int i = 0; i < weaponPickup.Count; i++)
        {
            InitializeNewPool(weaponPickup[i]);
        }

        for (int i = 0; i < buffPickups.Count; i++)
        {
            InitializeNewPool(buffPickups[i]);
        }
    }

    public GameObject GetObject(GameObject prefab, Transform target)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            InitializeNewPool(prefab);
        }

        GameObject objectToGet = poolDictionary[prefab].Get();

        PooledObject pooledObj = objectToGet.GetComponent<PooledObject>();
        if (pooledObj != null)
        {
            pooledObj.IncrementGeneration();
        }

        Vector3 targetPosition = target != null ? target.position : Vector3.zero;

        Rigidbody rb = objectToGet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        objectToGet.transform.parent = null;
        objectToGet.transform.position = targetPosition;
        objectToGet.transform.rotation = Quaternion.identity;

        objectToGet.SetActive(true);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        return objectToGet;
    }

    public void ReturnObject(GameObject objectToReturn, float delay = .001f)
    {
        PooledObject pooledObj = objectToReturn != null ? objectToReturn.GetComponent<PooledObject>() : null;
        int generationAtSchedule = pooledObj != null ? pooledObj.Generation : 0;

        ReturnObjectAsync(objectToReturn, delay, generationAtSchedule, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask ReturnObjectAsync(GameObject objectToReturn, float delay, int generationAtSchedule, CancellationToken ct = default)
    {
        try
        {
            // 일시정지(timeScale = 0) 중에도 반환 예약이 멈추지 않도록 unscaled 시간 사용
            await UniTask.WaitForSeconds(delay, ignoreTimeScale: true, cancellationToken: ct);

            if (objectToReturn == null || !objectToReturn.activeSelf)
                return;

            PooledObject pooledObj = objectToReturn.GetComponent<PooledObject>();
            if (pooledObj != null && pooledObj.Generation != generationAtSchedule)
                return;

            ReturnToPool(objectToReturn);
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[ObjectPool] 오브젝트 반환 작업 취소");
        }
    }

    private void ReturnToPool(GameObject objectToReturn)
    {
        if (objectToReturn == null)
            return;

        if (!objectToReturn.activeSelf)
        {
            return;
        }

        PooledObject pooledObj = objectToReturn.GetComponent<PooledObject>();

        if (pooledObj == null)
        {
            Debug.LogError($"[ObjectPool] '{objectToReturn.name}'에 PooledObject 컴포넌트가 없습니다. ObjectPool을 통해 생성되지 않은 객체입니다.");
            Destroy(objectToReturn);
            return;
        }

        GameObject originalPrefab = pooledObj.originalPrefab;

        if (!poolDictionary.ContainsKey(originalPrefab))
        {
            InitializeNewPool(originalPrefab);
        }

        poolDictionary[originalPrefab].Release(objectToReturn);
    }

    private void InitializeNewPool(GameObject prefab)
    {
        var capturedPrefab = prefab;
        var newPool = new ObjectPool<GameObject>(
            createFunc: () => CreateNewObject(capturedPrefab),
            actionOnGet: obj => { },
            actionOnRelease: obj =>
            {
                obj.SetActive(false);
                obj.transform.parent = transform;
            },
            actionOnDestroy: obj => Destroy(obj),
            defaultCapacity: poolSize,
            maxSize: maxPoolSize
        );

        poolDictionary[prefab] = newPool;

        Prewarm(newPool);
    }

    private void Prewarm(IObjectPool<GameObject> pool)
    {
        GameObject[] prewarmedObjects = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            prewarmedObjects[i] = pool.Get();
        }

        for (int i = 0; i < poolSize; i++)
        {
            pool.Release(prewarmedObjects[i]);
        }
    }

    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);

        PooledObject pooledObj = newObject.GetComponent<PooledObject>();
        if (pooledObj == null)
        {
            pooledObj = newObject.AddComponent<PooledObject>();
        }

        pooledObj.originalPrefab = prefab;
        newObject.SetActive(false);

        return newObject;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
