using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameManager : MonoBehaviour
{
    private enum GameMode
    {
        Normal,
        Blind
    }

    [Header("게임 설정")]
    [SerializeField] private GameMode curGameMode;
    [SerializeField, Range(4, 12)] private int animalCount;

    [Header("에셋")]
    [SerializeField] private AssetReference mainScene; // 메인 씬 에셋 레퍼런스
    [SerializeField] private AssetReference gameScene; // 게임 씬 에셋 레퍼런스 
    [SerializeField] private AssetReference[] animalSprites; // 애니멀 스프라이트들
    private AssetReference[] randomSprites; // 랜덤으로 선택된 애니멀 스프라이트들
    private Sprite[] animalSpritesArray; // 애니멀 스프라이트 배열

    // 컴퓨터 애니멀
    private Animal[] computerAnimals;

    // 플레이어 애니멀 & 슬롯
    private Animal[] playerAnimals;
    private Slot[] playerSlots;

    private Animal firstAnimal;
    private Animal secondAnimal;
    private List<Animal> canSwitchAnimals = new List<Animal>();

    private int switchCount; // 교환 횟수

    // 다시하기 위한 저장
    private Animal[] savedAnimals;
    private Vector2[] savedPositions;

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
        switchCount = 0;
        UIManager.Instance.SwitchCountText.text = switchCount.ToString();

        UIManager.Instance.SwitchTitleText.SetActive(true);
        UIManager.Instance.SwitchCountText.gameObject.SetActive(true);
        UIManager.Instance.ClearWindow.SetActive(false);

        SetSlotsAndAnimals();
    }

    // 슬롯 켜주고 컴퓨터, 플레이어 구분, 위치 선정
    private void SetSlotsAndAnimals()
    {
        // 컴퓨터 애니멀 담아둘 배열 생성
        computerAnimals = new Animal[animalCount];

        // 플레이어 애니멀 & 슬롯 담아둘 배열 생성
        playerAnimals = new Animal[animalCount];
        playerSlots = new Slot[animalCount];

        // 애니멀 스프라이트 배열 생성
        MakeAnimalSpritesArray();

        for (int i = 0; i < animalCount * 2; i++)
        {
            Animal animal = ObjectPoolManager.Instance.Get("Animal").GetComponent<Animal>();

            animal.OwnerType = (i % 2 != 0) ? OwnerType.Computer : OwnerType.Player; // 홀수 인덱스는 컴퓨터, 짝수 인덱스는 플레이어            

            if (i % 2 != 0) // 컴퓨터 : 홀수 인덱스 yPosition은 3f
            {
                animal.Index = (i - 1) / 2; // 홀수 인덱스는 0부터 시작하는 인덱스                

                SetAnimalSprites(animal); // 애니멀 스프라이트 설정
                SetAnimalPosition(animal, 3f);
                computerAnimals[animal.Index] = animal;

                if (curGameMode == GameMode.Blind) animal.gameObject.SetActive(false);
            }
            else // 짝수 인덱스 yPosition은 -2f
            {
                animal.Index = i / 2; // 플레이어 : 짝수 인덱스는 0부터 시작하는 인덱스                

                SetAnimalSprites(animal); // 애니멀 스프라이트 설정
                SetAnimalPosition(animal, -2f);
                playerAnimals[animal.Index] = animal;
            }
        }

        for (int i = 0; i < playerAnimals.Length; i++)
        {
            Slot slot = ObjectPoolManager.Instance.Get("Slot").GetComponent<Slot>();

            slot.Index = playerAnimals[i].Index;
            slot.transform.position = playerAnimals[i].transform.position;
            playerSlots[slot.Index] = slot;

            if (curGameMode == GameMode.Normal) slot.SpriteRenderer.gameObject.SetActive(false);
            else if (curGameMode == GameMode.Blind)
            {
                slot.SpriteRenderer.gameObject.SetActive(true);
                slot.SpriteRenderer.gameObject.transform.position = playerAnimals[i].transform.position + Vector3.up * 5f;
            }
        }

        ReleaseSpriteAssets(); // 사용한 스프라이트 에셋 해제

        ShufflePlayerAnimals(); // 플레이어 애니멀 랜덤 배치
    }

    // 애니멀 배치
    private void SetAnimalPosition(Animal animal, float yPosition)
    {
        float[] animalPositions = new float[animalCount];

        // 슬롯 수 홀수일 때
        if (animalCount % 2 != 0)
        {
            animalPositions[(animalCount - 1) / 2] = 0f; // 중앙 슬롯 위치 설정
            for (int i = 1; i <= animalCount / 2; i++)
            {
                animalPositions[(animalCount - 1) / 2 - i] = -i * 1.2f; // 왼쪽 슬롯 위치 설정
                animalPositions[(animalCount - 1) / 2 + i] = i * 1.2f; // 오른쪽 슬롯 위치 설정
            }
        }
        else // 슬롯 수 짝수일 때
        {
            for (int i = 1; i <= animalCount / 2; i++)
            {
                animalPositions[animalCount / 2 - i] = -i * 1.2f + 0.6f; // 왼쪽 슬롯 위치 설정
                animalPositions[animalCount / 2 + i - 1] = (i - 1) * 1.2f + 0.6f; // 오른쪽 슬롯 위치 설정
            }
        }

        animal.transform.position = new Vector2(animalPositions[animal.Index], yPosition);
    }

    // 애니멀 스프라이트 에셋에서 랜덤으로 가져와 animalCount 크기 배열 생성
    private void MakeAnimalSpritesArray()
    {
        randomSprites = animalSprites.OrderBy(x => UnityEngine.Random.value).Take(animalCount).ToArray();

        animalSpritesArray = new Sprite[randomSprites.Length];

        for (int i = 0; i < animalSpritesArray.Length; i++)
        {
            animalSpritesArray[i] = randomSprites[i].LoadAssetAsync<Sprite>().WaitForCompletion();
        }
    }

    // 애니멀 스프라이트 할당
    private void SetAnimalSprites(Animal animal)
    {
        animal.SpriteRenderer.sprite = animalSpritesArray[animal.Index];
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
    private void ShufflePlayerAnimals()
    {
        // 기존 위치 저장
        savedPositions = new Vector2[playerAnimals.Length];

        for (int i = 0; i < playerAnimals.Length; i++)
        {
            savedPositions[i] = playerAnimals[i].transform.position;
        }

        // playerAnimals 랜덤으로 섞기
        Animal[] shuffled;
        do
        {
            shuffled = playerAnimals.OrderBy(x => UnityEngine.Random.value).ToArray();
        }
        while (shuffled.SequenceEqual(playerAnimals));
        playerAnimals = shuffled;

        // 위치 재배치
        for (int i = 0; i < playerAnimals.Length; i++)
        {
            playerAnimals[i].transform.position = savedPositions[i];
        }

        // 다시하기 위한 저장
        savedAnimals = (Animal[])playerAnimals.Clone();
    }

    #endregion


    #region Game Methods

    public void Select(Animal animal)
    {
        if (firstAnimal == null) // 첫 번째 선택 애니멀 없다면
        {
            firstAnimal = animal; // 클릭한 것 첫 번째 선택 애니멀로
            animal.OnOffSelectedAnimation();

            UIManager.Instance.SelectedMarks.SetActive(true);
            UIManager.Instance.SelectedMarks.transform.position = Camera.main.WorldToScreenPoint(firstAnimal.transform.position + Vector3.up);

            if (firstAnimal == playerAnimals[0])
            {
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) + 1]); // 첫 번째 애니멀이 가장 왼쪽일 때
                UIManager.Instance.LeftMark.SetActive(false);
                UIManager.Instance.RightMark.SetActive(true);
            }
            else if (firstAnimal == playerAnimals[playerAnimals.Length - 1])
            {
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) - 1]); // 첫 번째 애니멀이 가장 오른쪽일 때
                UIManager.Instance.LeftMark.SetActive(true);
                UIManager.Instance.RightMark.SetActive(false);
            }
            else
            {
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) - 1]);
                canSwitchAnimals.Add(playerAnimals[Array.IndexOf(playerAnimals, animal) + 1]);
                UIManager.Instance.LeftMark.SetActive(true);
                UIManager.Instance.RightMark.SetActive(true);
            }
        }
        else // 첫 번째 선택 애니멀 있다면
        {
            if (canSwitchAnimals.Contains(animal)) // 양 옆 교환 가능한 애니멀들 중 하나 클릭했다면
            {
                secondAnimal = animal; // 클릭한 것 두 번째 선택 애니멀로
                animal.OnOffSelectedAnimation();
                SwitchAnimals(); // 위치, 배열 순서 교환
                GameClear().Forget(); // 클리어 판단
            }
            else
            {
                if (firstAnimal == animal) CancelSelect();
                else
                {
                    if (UIManager.Instance.WarningText.gameObject.activeSelf) return;

                    UIManager.Instance.WarningText.color = Color.white;
                    UIManager.Instance.WarningText.gameObject.SetActive(true);
                    UIManager.Instance.WarningText.DOFade(0f, 0.75f).SetEase(Ease.InExpo).onComplete += () => UIManager.Instance.WarningText.gameObject.SetActive(false);
                }
            }
        }
    }

    private void SwitchAnimals()
    {
        if (firstAnimal == null || secondAnimal == null) return;

        // 첫 번째 애니멀 선택 표시 해제
        UIManager.Instance.SelectedMarks.SetActive(false);

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
        firstAnimal.OnOffSelectedAnimation();
        secondAnimal.OnOffSelectedAnimation();

        firstAnimal = null;
        secondAnimal = null;
        canSwitchAnimals.Clear();

        switchCount++;
        UIManager.Instance.SwitchCountText.text = switchCount.ToString();
    }

    public void CancelSelect()
    {
        if (firstAnimal != null)
        {
            firstAnimal.OnOffSelectedAnimation();
            UIManager.Instance.SelectedMarks.SetActive(false);
            firstAnimal = null;
        }
    }

    private async UniTask GameClear()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5d));

        if (playerSlots.All(x => x.Index == x.CurAnimal.Index))
        {
            UIManager.Instance.SwitchTitleText.SetActive(false);
            UIManager.Instance.SwitchCountText.gameObject.SetActive(false);
            UIManager.Instance.ClearText.SetActive(true);

            if (curGameMode == GameMode.Blind)
            {
                for (int i = 0; i < animalCount; i++)
                {
                    playerSlots[i].SpriteRenderer.gameObject.SetActive(false);
                    computerAnimals[i].gameObject.SetActive(true);
                }
            }

            ShowClearScreen().Forget();
        }
    }

    private async UniTask ShowClearScreen()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2d));

        for (int i = 0; i < animalCount; i++)
        {
            computerAnimals[i].gameObject.SetActive(false);
            playerAnimals[i].gameObject.SetActive(false);
            playerSlots[i].gameObject.SetActive(false);
        }

        UIManager.Instance.ClearText.SetActive(false);
        UIManager.Instance.ClearWindow.SetActive(true);
    }

    #endregion

    public void GoMainScene()
    {
        SceneLoadManager.Instance.LoadScene(mainScene);
        UIManager.Instance.ClearWindow.SetActive(false);
    }

    public void GoGameScene()
    {
        SceneLoadManager.Instance.LoadScene(gameScene);
    }

    public void Retry()
    {
        switchCount = 0;
        UIManager.Instance.SwitchCountText.text = switchCount.ToString();

        UIManager.Instance.SwitchTitleText.SetActive(true);
        UIManager.Instance.SwitchCountText.gameObject.SetActive(true);
        UIManager.Instance.ClearWindow.SetActive(false);

        // 기존 애니멀 배열로 다시 배치
        playerAnimals = (Animal[])savedAnimals.Clone();

        for (int i = 0; i < animalCount; i++)
        {
            playerAnimals[i].transform.position = savedPositions[i];
            playerAnimals[i].gameObject.SetActive(true);
            playerSlots[i].gameObject.SetActive(true);

            if (curGameMode == GameMode.Normal) computerAnimals[i].gameObject.SetActive(true);
            else if (curGameMode == GameMode.Blind) playerSlots[i].SpriteRenderer.gameObject.SetActive(true);
        }
    }

    public void SetGameMode()
    {
        if (curGameMode == GameMode.Normal) curGameMode = GameMode.Blind;
        else if (curGameMode == GameMode.Blind) curGameMode = GameMode.Normal;

        UIManager.Instance.ModeTextBG.text = UIManager.Instance.ModeText.text = Enum.GetName(typeof(GameMode), curGameMode);
    }

    public void PlusAnimalCount()
    {
        UIManager.Instance.CountRightBtn.SetActive(true);
        animalCount++;
        UIManager.Instance.AnimalCountTextBG.text = UIManager.Instance.AnimalCountText.text = animalCount.ToString();

        if (animalCount == 12) UIManager.Instance.CountRightBtn.SetActive(false);
        else if (animalCount == 5) UIManager.Instance.CountLeftBtn.SetActive(true);
    }

    public void MinusAnimalCount()
    {        
        animalCount--;
        UIManager.Instance.AnimalCountTextBG.text = UIManager.Instance.AnimalCountText.text = animalCount.ToString();

        if (animalCount == 4) UIManager.Instance.CountLeftBtn.SetActive(false);
        else if (animalCount == 11) UIManager.Instance.CountRightBtn.SetActive(true);
    }
}
