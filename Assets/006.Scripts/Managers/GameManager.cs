using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Range(3, 6)] public int slotCount;

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
        SetSlotsAndAnimals();
    }

    // 슬롯 켜주고 컴퓨터, 플레이어 구분, 위치 선정
    public void SetSlotsAndAnimals()
    {
        for (int i = 0; i < slotCount * 2; i++)
        {
            Slot slot = ObjectPoolManager.Instance.Get("Slot").GetComponent<Slot>();
            Animal animal = ObjectPoolManager.Instance.Get("Animal").GetComponent<Animal>();

            slot.ownerType = (i % 2 != 0) ? OwnerType.Computer : OwnerType.Player; // 홀수 인덱스는 컴퓨터, 짝수 인덱스는 플레이어
            animal.ownerType = slot.ownerType; // 슬롯과 동일한 소유자 타입 설정

            if (i % 2 != 0) // 홀수 인덱스 yPosition은 3f
            {
                slot.index = (i - 1) / 2; // 홀수 인덱스는 0부터 시작하는 인덱스
                animal.index = slot.index; // 동물 인덱스도 동일하게 설정

                SetSlotPosition(slot, 3f);
            }
            else // 짝수 인덱스 yPosition은 -2f
            {
                slot.index = i / 2; // 짝수 인덱스는 0부터 시작하는 인덱스
                animal.index = slot.index; // 동물 인덱스도 동일하게 설정

                SetSlotPosition(slot, -2f);
            }

            animal.transform.position = slot.transform.position; // 동물 위치를 슬롯 위치로 설정
        }
    }

    // 슬롯 배치
    public void SetSlotPosition(Slot slot, float yPosition)
    {
        float[] slotPositions = new float[slotCount];

        // 슬롯 수 홀수일 때
        if (slotCount % 2 != 0)
        {
            slotPositions[(slotCount - 1) / 2] = 0f; // 중앙 슬롯 위치 설정
            for (int i = 1; i <= slotCount / 2; i++)
            {
                slotPositions[(slotCount - 1) / 2 - i] = -i * 1.2f; // 왼쪽 슬롯 위치 설정
                slotPositions[(slotCount - 1) / 2 + i] = i * 1.2f; // 오른쪽 슬롯 위치 설정
            }
        }
        else // 슬롯 수 짝수일 때
        {
            slotPositions[slotCount / 2] = -0.6f; // 중앙 왼쪽 슬롯 위치 설정
            for (int i = 1; i <= slotCount / 2; i++)
            {
                slotPositions[slotCount / 2 - i] = -i * 1.2f; // 왼쪽 슬롯 위치 설정
                slotPositions[slotCount / 2 + i] = i * 1.2f; // 오른쪽 슬롯 위치 설정
            }
        }

        slot.transform.position = new Vector2(slotPositions[slot.index], yPosition);
    }    

    public void StartGame()
    {
        SceneLoadManager.Instance.LoadScene(SceneLoadManager.Instance.mainScene);

        Initialize();
    }
    
    #endregion
}
