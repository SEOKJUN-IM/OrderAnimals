using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("게임 UI")]
    [SerializeField] private TextMeshProUGUI warningText;
    public TextMeshProUGUI WarningText { get => warningText; }

    [SerializeField] private GameObject selectedMarks;
    public GameObject SelectedMarks { get => selectedMarks; }

    [SerializeField] private GameObject leftMark;
    public GameObject LeftMark { get => leftMark; }

    [SerializeField] private GameObject rightMark;
    public GameObject RightMark { get => rightMark; }

    [SerializeField] private GameObject switchTitleText;
    public GameObject SwitchTitleText { get => switchTitleText; }

    [SerializeField] private TextMeshProUGUI switchCountText;
    public TextMeshProUGUI SwitchCountText { get => switchCountText; }

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
