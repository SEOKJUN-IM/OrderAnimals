using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable
{
    public ObjectPool<GameObject> Pool { get; set; }
    public GameObject self { get; }
}
