using UnityEngine;
using UnityEngine.Pool;

public class Slot : MonoBehaviour, IPoolable
{
    private ObjectPool<GameObject> pool;
    public ObjectPool<GameObject> Pool { get => pool; set => pool = value; }

    public GameObject self => this.gameObject;
}
