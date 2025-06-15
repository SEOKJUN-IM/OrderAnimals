using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

public class Animal : MonoBehaviour, IPoolable, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ObjectPool<GameObject> pool;
    public ObjectPool<GameObject> Pool { get => pool; set => pool = value; }

    public GameObject self => this.gameObject;

    [Header("Type")]
    private OwnerType ownerType; // 소유자 타입 (컴퓨터 또는 플레이어)
    public OwnerType OwnerType { get => ownerType; set => ownerType = value; }

    private int index;
    public int Index { get => index; set => index = value; }

    [Header("Animal Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }

    [SerializeField] private Animator animator;    

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ownerType == OwnerType.Computer) return; // 컴퓨터 소유의 동물은 클릭할 수 없음

        GameManager.Instance.Select(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ownerType == OwnerType.Computer) return; // 컴퓨터 소유의 동물은 클릭할 수 없음
        animator.SetBool("OnPointer", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ownerType == OwnerType.Computer) return; // 컴퓨터 소유의 동물은 클릭할 수 없음
        animator.SetBool("OnPointer", false);
    }

    public void OnOffSelectedAnimation()
    {
        animator.SetBool("Selected", !animator.GetBool("Selected"));
    }
}
