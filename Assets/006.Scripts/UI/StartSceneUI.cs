using UnityEngine;
using UnityEngine.UI;

public class StartSceneUI : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;

    void Awake()
    {
        gameStartBtn.onClick.AddListener(GameManager.Instance.GoGameScene);
    }
}
