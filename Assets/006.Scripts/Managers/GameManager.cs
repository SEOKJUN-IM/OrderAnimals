using UnityEngine;

public class GameManager : MonoBehaviour
{    
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) _instance = new GameObject("GameManager").AddComponent<GameManager>();
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

    #region Public Methods
    /// <summary>
    /// 게임 초기화를 위한 메서드
    /// </summary>
    public void Initialize()
    {
        
    }

    
    #endregion
}
