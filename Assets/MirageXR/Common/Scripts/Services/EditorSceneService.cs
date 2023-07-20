using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using MirageXR;
using UnityEngine.SceneManagement;

public class EditorSceneService
{
    private static BrandManager brandManager => RootObject.Instance.brandManager;

    private Scene editorScene;

    public async Task LoadEditorAsync()
    {
        await UnloadExistingScene();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        await SceneManager.LoadSceneAsync(PlatformManager.Instance.PlayerSceneName, LoadSceneMode.Additive);
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

        EventManager.NotifyEditorLoaded();
    }

    public async Task UnloadExistingScene()
    {
        if (editorScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(editorScene);
        }

        EventManager.NotifyEditorUnloaded();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (brandManager.Customizable)
        {
            brandManager.AddCustomColors();
        }

        if (scene.name == PlatformManager.Instance.PlayerSceneName)
        {
            editorScene = scene;
        }
    }
}
