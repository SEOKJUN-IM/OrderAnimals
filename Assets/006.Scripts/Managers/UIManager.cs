using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("게임 UI")]
    public TextMeshProUGUI warningText;
    public GameObject selectedMarks;
    public GameObject leftMark;
    public GameObject rightMark;
    public GameObject switchTitleText;
    public TextMeshProUGUI switchCountText;

    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null) _instance = new GameObject("UIManager").AddComponent<UIManager>();
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
    }
}
