using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] private int poolSize = 10;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = 
        new Dictionary<GameObject, Queue<GameObject>>();


    [SerializeField] private List<GameObject> weaponPickup = new List<GameObject>();
    [SerializeField] private GameObject ammoPickup;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else 
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeNewPool(ammoPickup);
        for (int i = 0; i < weaponPickup.Count; i++)
        {
            InitializeNewPool(weaponPickup[i]);
        }
    }

    public GameObject GetObject(GameObject prefab, Transform target)
    {
        if (poolDictionary.ContainsKey(prefab) == false)
        {
            InitializeNewPool(prefab);
        }

        if (poolDictionary[prefab].Count == 0)
            CreateNewObject(prefab);

        GameObject objectToGet = poolDictionary[prefab].Dequeue();

        objectToGet.transform.position = target.position;
        objectToGet.transform.parent = null;

        objectToGet.SetActive(true);

        return objectToGet;
    }

    public void ReturnObject(GameObject objectToReturn, float delay = .001f)
    {
        StartCoroutine(DelayReturn(delay, objectToReturn));
    }

    private IEnumerator DelayReturn(float delay,GameObject objectToReturn)
    {
        yield return new WaitForSeconds(delay);

        ReturnToPool(objectToReturn);
    }

    private void ReturnToPool(GameObject objectToReturn)
    {
        PooledObject pooledObj = objectToReturn.GetComponent<PooledObject>();
        
        if (pooledObj == null)
        {
            Debug.LogError($"[ObjectPool] '{objectToReturn.name}'에 PooledObject 컴포넌트가 없습니다. ObjectPool을 통해 생성되지 않은 객체입니다.");
            Destroy(objectToReturn);
            return;
        }

        GameObject originalPrefab = pooledObj.originalPrefab;

        if (poolDictionary.ContainsKey(originalPrefab) == false)
        {
            Debug.LogWarning($"[ObjectPool] '{originalPrefab.name}' 풀이 존재하지 않아 새로 생성합니다.");
            InitializeNewPool(originalPrefab);
        }

        objectToReturn.SetActive(false);
        objectToReturn.transform.parent = transform;
        
        poolDictionary[originalPrefab].Enqueue(objectToReturn);
    }

    private void InitializeNewPool(GameObject prefab)
    {
        poolDictionary[prefab] = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject(prefab);
        }
    }

    private void CreateNewObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);
        
        PooledObject pooledObj = newObject.GetComponent<PooledObject>();
        if (pooledObj == null)
        {
            pooledObj = newObject.AddComponent<PooledObject>();
        }
        
        pooledObj.originalPrefab = prefab;
        newObject.SetActive(false);

        poolDictionary[prefab].Enqueue(newObject);
    }
}
