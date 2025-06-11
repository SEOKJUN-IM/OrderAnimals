using UnityEngine;
using UnityEngine.EventSystems;

public class CancelScreen : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.CancelSelect();
    }    
}
