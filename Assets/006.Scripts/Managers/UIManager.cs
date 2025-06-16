using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("메인 UI")]
    [SerializeField] private GameObject mainCanvas;
    public GameObject MainCanvas { get => mainCanvas; }    

    [SerializeField] private TextMeshProUGUI modeText;
    public TextMeshProUGUI ModeText { get => modeText; }    

    [SerializeField] private TextMeshProUGUI animalCountText;
    public TextMeshProUGUI AnimalCountText { get => animalCountText; }

    [SerializeField] private GameObject countLeftBtn;
    public GameObject CountLeftBtn { get => countLeftBtn; }

    [SerializeField] private GameObject countRightBtn;
    public GameObject CountRightBtn { get => countRightBtn; }

    [Header("게임 UI")]
    [SerializeField] private TextMeshProUGUI warningText;
    public TextMeshProUGUI WarningText { get => warningText; }

    [SerializeField] private GameObject selectedMarks;
    public GameObject SelectedMarks { get => selectedMarks; }

    [SerializeField] private GameObject leftMark;
    public GameObject LeftMark { get => leftMark; }

    [SerializeField] private GameObject rightMark;
    public GameObject RightMark { get => rightMark; }

    [SerializeField] private GameObject countTexts;
    public GameObject CountTexts { get => countTexts; }

    [SerializeField] private TextMeshProUGUI minSwitchCountText;
    public TextMeshProUGUI MinSwitchCountText { get => minSwitchCountText; }

    [SerializeField] private TextMeshProUGUI switchCountText;
    public TextMeshProUGUI SwitchCountText { get => switchCountText; }

    [SerializeField] private TextMeshProUGUI maxSwitchCountText;
    public TextMeshProUGUI MaxSwitchCountText { get => maxSwitchCountText; }

    [Header("게임클리어 UI")]
    [SerializeField] private GameObject clearText;
    public GameObject ClearText { get => clearText; }

    [SerializeField] private GameObject clearWindow;
    public GameObject ClearWindow { get => clearWindow; }

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
