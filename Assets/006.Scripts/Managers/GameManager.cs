using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AssetReference mainScene; // 메인 씬 에셋 레퍼런스    

    [Header("게임 설정")]
    [Range(4, 12)] public int slotCount;

    [Header("게임 UI")]
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private GameObject selectedMarks;
    [SerializeField] private GameObject leftMark;
    [SerializeField] private GameObject rightMark;

    // 애니멀 스프라이트 랜덤 할당
    [SerializeField] private AssetReference[] animalSprites; // 애니멀 스프라이트들
    private AssetReference[] randomSprites; // 랜덤으로 선택된 애니멀 스프라이트들
    private Sprite[] animalSpritesArray; // 애니멀 스프라이트 배열

    // 플레이어 애니멀
    private Animal[] playerAnimals;

    private Animal firstAnimal;
    private Animal secondAnimal;
    private List<Animal> canSwitchAnimals = new List<Animal>();

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
        // 플레이어 애니멀 담아둘 배열 생성
        playerAnimals = new Animal[slotCount];

        // 애니멀 스프라이트 배열 생성
        MakeAnimalSpritesArray();

        for (int i = 0; i < slotCount * 2; i++)
        {
            Slot slot = ObjectPoolManager.Instance.Get("Slot").GetComponent<Slot>();
            Animal animal = ObjectPoolManager.Instance.Get("Animal").GetComponent<Animal>();

            slot.ownerType = (i % 2 != 0) ? OwnerType.Computer : OwnerType.Player; // 홀수 인덱스는 컴퓨터, 짝수 인덱스는 플레이어
            animal.ownerType = slot.ownerType; // 슬롯과 동일한 소유자 타입 설정

            if (i % 2 != 0) // 컴퓨터 애니멀 : 홀수 인덱스 yPosition은 3f
            {
                slot.index = (i - 1) / 2; // 홀수 인덱스는 0부터 시작하는 인덱스
                animal.index = slot.index; // 동물 인덱스도 동일하게 설정

                SetAnimalSprites(animal); // 애니멀 스프라이트 설정
                SetSlotPosition(slot, 3f);
            }
            else // 짝수 인덱스 yPosition은 -2f
            {
                slot.index = i / 2; // 플레이어 애니멀 : 짝수 인덱스는 0부터 시작하는 인덱스
                animal.index = slot.index; // 동물 인덱스도 동일하게 설정

                SetAnimalSprites(animal); // 애니멀 스프라이트 설정
                SetSlotPosition(slot, -2f);
                playerAnimals[animal.index] = animal;
            }

            animal.transform.position = slot.transform.position; // 동물 위치를 슬롯 위치로 설정            
        }

        ReleaseSpriteAssets(); // 사용한 스프라이트 에셋 해제

        RandomPlayerAnimals(); // 플레이어 애니멀 랜덤 배치
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

    // 애니멀 스프라이트 에셋에서 랜덤으로 가져와 slotCount 크기 배열 생성
    private void MakeAnimalSpritesArray()
    {
        randomSprites = animalSprites.OrderBy(x => UnityEngine.Random.value).Take(slotCount).ToArray();

        animalSpritesArray = new Sprite[randomSprites.Length];

        for (int i = 0; i < animalSpritesArray.Length; i++)
        {
            animalSpritesArray[i] = randomSprites[i].LoadAssetAsync<Sprite>().WaitForCompletion();
        }
    }

    // 애니멀 스프라이트 할당
    private void SetAnimalSprites(Animal animal)
    {
        animal.spriteRenderer.sprite = animalSpritesArray[animal.index];
    }

    // 애니멀 스프라이트 할당 후 에셋 릴리즈
    private void ReleaseSpriteAssets()
    {
        for (int i = 0; i < randomSprites.Length; i++)
        {
            randomSprites[i].ReleaseAsset();
        }
    }

    // 플레이어 애니멀 배열 랜덤 섞기 & 맞게 배치
    private void RandomPlayerAnimals()
    {
        // 기존 위치 저장
        Vector2[] tempPos = new Vector2[playerAnimals.Length];

        for (int i = 0; i < playerAnimals.Length; i++)
        {
            tempPos[i] = playerAnimals[i].transform.position;
        }

        // playerAnimals 랜덤으로 섞기
        playerAnimals = playerAnimals.OrderBy(x => UnityEngine.Random.value).Take(playerAnimals.Length).ToArray();

        for (int i = 0; i < playerAnimals.Length; i++)
        {
            playerAnimals[i].transform.position = tempPos[i]; // 재배치
        }
    }    

    #endregion


    #region Game Methods

    public void Select(Animal animal)
    {
        if (firstAnimal == null) // 첫 번째 선택 애니멀 없다면
        {
            firstAnimal = animal; // 클릭한 것 첫 번째 선택 애니멀로
            animal.animator.SetBool("Selected", !animal.animator.GetBool("Selected"));

            selectedMarks.SetActive(true);
            selectedMarks.transform.position = Camera.main.WorldToScreenPoint(firstAnimal.transform.position + Vector3.up);

            if (firstAnimal == playerAnimals[0])
            {
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) + 1]); // 첫 번째 애니멀이 가장 왼쪽일 때
                leftMark.SetActive(false);
                rightMark.SetActive(true);
            }
            else if (firstAnimal == playerAnimals[playerAnimals.Length - 1])
            {
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) - 1]); // 첫 번째 애니멀이 가장 오른쪽일 때
                leftMark.SetActive(true);
                rightMark.SetActive(false);
            }
            else
            {
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) - 1]);
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) + 1]);
                leftMark.SetActive(true);
                rightMark.SetActive(true);
            }
        }
        else // 첫 번째 선택 애니멀 있다면
        {
            if (canSwitchAnimals.Contains(animal)) // 양 옆 교환 가능한 애니멀들 중 하나 클릭했다면
            {
                secondAnimal = animal; // 클릭한 것 두 번째 선택 애니멀로
                animal.animator.SetBool("Selected", !animal.animator.GetBool("Selected"));
                SwitchAnimals(); // 위치, 배열 순서 교환
            }
            else
            {
                if (firstAnimal == animal) CancelSelect();
                else
                {
                    if (warningText.gameObject.activeSelf) return;

                    warningText.color = Color.white;
                    warningText.gameObject.SetActive(true);
                    warningText.DOFade(0f, 0.75f).SetEase(Ease.InExpo).onComplete += () => warningText.gameObject.SetActive(false);                   
                }
            }
        }
    }

    public void SwitchAnimals()
    {
        if (firstAnimal == null || secondAnimal == null) return;

        // 첫 번째 애니멀 선택 표시 해제
        selectedMarks.SetActive(false);

        // 첫 번째 애니멀과 두 번째 애니멀의 위치를 교환
        Vector2 tempPosition = firstAnimal.transform.position;
        firstAnimal.transform.DOMove(secondAnimal.transform.position, 0.5f).SetEase(Ease.OutBack);
        secondAnimal.transform.DOMove(tempPosition, 0.5f).SetEase(Ease.OutBack);

        // 첫 번째 애니멀과 두 번째 애니멀의 playerAnimals 내 순서를 교환
        int firstIndex = Array.IndexOf(playerAnimals, firstAnimal);
        int secondIndex = Array.IndexOf(playerAnimals, secondAnimal);
        playerAnimals[firstIndex] = secondAnimal;
        playerAnimals[secondIndex] = firstAnimal;        

        // 첫 번째 애니멀과 두 번째 애니멀을 초기화
        firstAnimal.animator.SetBool("Selected", false);
        secondAnimal.animator.SetBool("Selected", false);
        
        firstAnimal = null;
        secondAnimal = null;
        canSwitchAnimals.Clear();
    }

    public void CancelSelect()
    {
        if (firstAnimal != null)
        {
            firstAnimal.animator.SetBool("Selected", false);
            selectedMarks.SetActive(false);
            firstAnimal = null;
        }
    }    

    #endregion

    public void StartGame()
    {
        SceneLoadManager.Instance.LoadScene(mainScene);
    }
}
