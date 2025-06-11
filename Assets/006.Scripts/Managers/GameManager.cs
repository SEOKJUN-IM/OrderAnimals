using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AssetReference mainScene; // 메인 씬 에셋 레퍼런스    

    [Header("게임 설정")]
    [Range(4, 8)] public int slotCount;

    // 애니멀 스프라이트 랜덤 할당
    [SerializeField] private AssetReference[] animalSprites; // 애니멀 스프라이트들
    private AssetReference[] randomSprites; // 랜덤으로 선택된 애니멀 스프라이트들
    private Sprite[] animalSpritesArray; // 애니멀 스프라이트 배열

    // 플레이어 애니멀
    private Animal[] playerAnimals;

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

    #region Initial Settings

    /// <summary>
    /// 게임 초기화를 위한 메서드
    /// </summary>
    public void Initialize()
    {
        SetSlotsAndAnimals();
    }

    // 슬롯 켜주고 컴퓨터, 플레이어 구분, 위치 선정
    private void SetSlotsAndAnimals()
    {
        // 애니멀 스프라이트 배열 생성
        MakeAnimalSpritesArray();

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

                SetAnimalSprites(animal); // 애니멀 스프라이트 설정
                SetSlotPosition(slot, 3f);
            }
            else // 짝수 인덱스 yPosition은 -2f
            {
                slot.index = i / 2; // 짝수 인덱스는 0부터 시작하는 인덱스
                animal.index = slot.index; // 동물 인덱스도 동일하게 설정

                SetAnimalSprites(animal); // 애니멀 스프라이트 설정
                SetSlotPosition(slot, -2f);
            }

            animal.transform.position = slot.transform.position; // 동물 위치를 슬롯 위치로 설정
            SetPlayerAnimals(animal); // 플레이어 애니멀 설정
        }

        ReleaseSpriteAssets(); // 사용한 스프라이트 에셋 해제
    }

    // 슬롯 배치
    private void SetSlotPosition(Slot slot, float yPosition)
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
            for (int i = 1; i <= slotCount / 2; i++)
            {
                slotPositions[slotCount / 2 - i] = -i * 1.2f + 0.6f; // 왼쪽 슬롯 위치 설정
                slotPositions[slotCount / 2 + i - 1] = (i - 1) * 1.2f + 0.6f; // 오른쪽 슬롯 위치 설정
            }
        }

        slot.transform.position = new Vector2(slotPositions[slot.index], yPosition);
    }

    private void MakeAnimalSpritesArray()
    {
        randomSprites = animalSprites.OrderBy(x => Random.value).Take(slotCount).ToArray();

        animalSpritesArray = new Sprite[randomSprites.Length];

        for (int i = 0; i < animalSpritesArray.Length; i++)
        {
            animalSpritesArray[i] = randomSprites[i].LoadAssetAsync<Sprite>().WaitForCompletion();
        }
    }

    private void SetAnimalSprites(Animal animal)
    {
        animal.spriteRenderer.sprite = animalSpritesArray[animal.index];
    }

    private void ReleaseSpriteAssets()
    {
        for (int i = 0; i < randomSprites.Length; i++)
        {
            randomSprites[i].ReleaseAsset();
        }
    }

    private void SetPlayerAnimals(Animal animal)
    {
        playerAnimals = new Animal[slotCount];

        for (int i = 0; i < playerAnimals.Length; i++)
        {
            if (animal.ownerType == OwnerType.Player && animal.index == i) playerAnimals[i] = animal;
        }
    }

    #endregion


    #region Game Methods

    public void MoveRight()
    {

    }

    public void MoveLeft()
    {
        
    }

    #endregion

    public void StartGame()
    {
        SceneLoadManager.Instance.LoadScene(mainScene);
    }
}
