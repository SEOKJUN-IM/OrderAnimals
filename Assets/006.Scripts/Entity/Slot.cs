using System;
using UnityEngine;
using UnityEngine.Pool;

public class Slot : MonoBehaviour, IPoolable
{
    private ObjectPool<GameObject> pool;
    public ObjectPool<GameObject> Pool { get => pool; set => pool = value; }

    public GameObject self => this.gameObject;    

    public int index;

    public SpriteRenderer spriteRenderer;

    public Animal curAnimal;    

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Animal animal))
        {
            curAnimal = animal;

            if (curAnimal.index == index) spriteRenderer.color = Color.green;
            else if (Mathf.Abs(curAnimal.index - index) == 1) spriteRenderer.color = Color.orange;
            else spriteRenderer.color = Color.red;            
        }
    }
}
