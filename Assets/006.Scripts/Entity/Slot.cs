using UnityEngine;
using UnityEngine.Pool;

public class Slot : MonoBehaviour, IPoolable
{
    private ObjectPool<GameObject> pool;
    public ObjectPool<GameObject> Pool { get => pool; set => pool = value; }

    public GameObject self => this.gameObject;    

    private int index;
    public int Index { get => index; set => index = value; }

    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }

    private Animal curAnimal;
    public Animal CurAnimal { get => curAnimal; }   

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Animal animal))
        {
            curAnimal = animal;

            if (curAnimal.Index == index) spriteRenderer.color = Color.green;
            else if (Mathf.Abs(curAnimal.Index - index) == 1) spriteRenderer.color = Color.orange;
            else spriteRenderer.color = Color.red;            
        }
    }
}
