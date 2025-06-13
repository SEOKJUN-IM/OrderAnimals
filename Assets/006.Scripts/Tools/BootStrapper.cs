using UnityEngine;
using UnityEngine.AddressableAssets;

public class BootStrapper : MonoBehaviour
{
    [SerializeField] private AssetReference startScene;

    void Start()
    {
        SceneLoadManager.Instance.LoadScene(startScene);
    }
}
