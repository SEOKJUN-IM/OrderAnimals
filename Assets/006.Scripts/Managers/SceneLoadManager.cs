using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    [SerializeField] private string mainSceneName; // 메인 씬 이름
    [SerializeField] private string gameSceneName; // 게임 씬 이름

    private static SceneLoadManager _instance;
    public static SceneLoadManager Instance
    {
        get
        {
            if (_instance == null) _instance = new GameObject("SceneLoadManager").AddComponent<SceneLoadManager>();
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

    private async UniTask LoadSceneAsync(AssetReference sceneReference)
    {
        var sceneHandle = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Single, false);

        await UniTask.WaitUntil(() => sceneHandle.Status == AsyncOperationStatus.Succeeded);

        if (sceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            SceneInstance sceneInstance = sceneHandle.Result;

            await sceneInstance.ActivateAsync();

            if (sceneInstance.Scene.isLoaded)
            {
                if (sceneInstance.Scene.name == mainSceneName)
                {
                    UIManager.Instance.MainCanvas.SetActive(true);
                }
                else if (sceneInstance.Scene.name == gameSceneName)
                {
                    UIManager.Instance.MainCanvas.SetActive(false);
                    GameManager.Instance.Initialize();
                }                
            }           
        }               
    }

    public void LoadScene(AssetReference sceneReference)
    {
        LoadSceneAsync(sceneReference).Forget();
    }
}
