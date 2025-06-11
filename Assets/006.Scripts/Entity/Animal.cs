using UnityEngine;
using UnityEngine.Pool;

public class Animal : MonoBehaviour, IPoolable
{
    private ObjectPool<GameObject> pool;
    public ObjectPool<GameObject> Pool { get => pool; set => pool = value; }

    public GameObject self => this.gameObject;

    [Header("Type")]
    public OwnerType ownerType; // 소유자 타입 (컴퓨터 또는 플레이어)

    public int index;
}
