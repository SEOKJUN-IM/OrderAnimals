using UnityEngine;
using UnityEngine.AddressableAssets;

public class BootStrapper : MonoBehaviour
{
    [SerializeField] private AssetReference mainScene;

    void Start()
    {
        SceneLoadManager.Instance.LoadScene(mainScene);
    }
}
