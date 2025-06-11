using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private int defaultCapacity;
    [SerializeField] private int maxPoolSize;

    public GameObject[] poolObjPrefabs; // 프리팹 배열

    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();
    private Dictionary<GameObject, GameObject> poolContainers = new Dictionary<GameObject, GameObject>();

    private static ObjectPoolManager _instance;
    public static ObjectPoolManager Instance
    {
        get
        {
            if (_instance == null) _instance = new GameObject("ObjectPoolManager").AddComponent<ObjectPoolManager>();
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (_instance != this) Destroy(gameObject);
        }

        Initialize();
    }

    public void Initialize()
    {
        foreach (var prefab in poolObjPrefabs)
        {
            if (prefab == null) continue;

            var pool = new ObjectPool<GameObject>(
                () => CreatePooledObject(prefab),
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true,
                defaultCapacity,
                maxPoolSize
            );
            pools[prefab] = pool;

            // 미리 풀 생성
            for (int i = 0; i < defaultCapacity; i++)
            {
                var obj = CreatePooledObject(prefab);
                pool.Release(obj);
            }
        }
    }

    // 오브젝트 풀에서 오브젝트 가져오기
    public GameObject Get(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            return pool.Get();
        }
        Debug.LogError($"[ObjectPoolManager] 해당 프리팹({prefab.name})에 대한 풀을 찾을 수 없습니다.");
        return null;
    }

    public GameObject Get(string prefabName)
    {
        foreach (var prefab in poolObjPrefabs)
        {
            if (prefab != null && prefab.name == prefabName)
            {
                return Get(prefab);
            }
        }
        Debug.LogError($"[ObjectPoolManager] {prefabName} 프리팹을 찾을 수 없습니다.");
        return null;
    }


    // 오브젝트 풀에 오브젝트 반환
    public void Release(GameObject prefab, GameObject obj)
    {
        if (pools.TryGetValue(prefab, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            Debug.LogError($"[ObjectPoolManager] 해당 프리팹({prefab.name})에 대한 풀을 찾을 수 없습니다.");
            Destroy(obj);
        }
    }

    // 생성
    private GameObject CreatePooledObject(GameObject prefab)
    {
        if (!poolContainers.ContainsKey(prefab) || poolContainers[prefab] == null)
        {
            GameObject container = new GameObject($"[PoolContainer] {prefab.name}");
            container.transform.SetParent(transform);            
            poolContainers[prefab] = container;
        }

        GameObject poolObj = Instantiate(prefab, poolContainers[prefab].transform);
        IPoolable poolable = poolObj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.Pool = pools[prefab];
        }
        return poolObj;
    }

    // 사용
    private void OnTakeFromPool(GameObject poolObj)
    {
        poolObj.SetActive(true);
    }

    // 반환
    private void OnReturnToPool(GameObject poolObj)
    {
        poolObj.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolObj)
    {
        Destroy(poolObj);
    }
}