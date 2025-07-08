using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

[System.Serializable]
public class PoolingData
{
    [SerializeField] private GameObject poolObjPrefab;
    public GameObject PoolObjPrefab => poolObjPrefab;

    [SerializeField] private int defaultCapacity;
    public int DefaultCapacity => defaultCapacity;
}

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private int maxPoolSize;

    [SerializeField] private PoolingData[] poolingDatas;

    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();
    private Dictionary<string, GameObject> poolContainers = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> PoolContainers => poolContainers;

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
        foreach (var datas in poolingDatas)
        {
            if (datas.PoolObjPrefab == null) continue;

            var pool = new ObjectPool<GameObject>(
                () => CreatePooledObject(datas.PoolObjPrefab),
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true,
                datas.DefaultCapacity,
                maxPoolSize
            );
            pools[datas.PoolObjPrefab] = pool;

            // 미리 풀 생성
            for (int i = 0; i < datas.DefaultCapacity; i++)
            {
                var obj = CreatePooledObject(datas.PoolObjPrefab);
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
        foreach (var data in poolingDatas)
        {
            if (data.PoolObjPrefab != null && data.PoolObjPrefab.name == prefabName)
            {
                return Get(data.PoolObjPrefab);
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

    // active 오브젝트 모두 풀에 반환
    public void ReleaseAllActive()
    {
        IPoolable[] actives = GetComponentsInChildren<IPoolable>(false);

        foreach (IPoolable active in actives)
        {
            active.Pool.Release(active.Self);
        }
    }

    // 생성
    private GameObject CreatePooledObject(GameObject prefab)
    {
        if (!poolContainers.ContainsKey(prefab.name) || poolContainers[prefab.name] == null)
        {
            GameObject container = new GameObject($"[PoolContainer] {prefab.name}");
            container.transform.SetParent(transform);
            poolContainers.Add(prefab.name, container);
        }

        GameObject poolObj = Instantiate(prefab, poolContainers[prefab.name].transform);
        poolObj.name = prefab.name;

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
        if (poolObj.transform.parent != poolContainers[poolObj.name]) poolObj.transform.SetParent(poolContainers[poolObj.name].transform);

        poolObj.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolObj)
    {
        Destroy(poolObj);
    }
}