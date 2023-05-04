using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using MirageXR;
using UnityEngine.SceneManagement;
using i5.Toolkit.Core.VerboseLogging;

public class EditorSceneService
{
    private Scene editorScene;

    public async Task LoadEditorAsync()
    {
        AppLog.LogInfo("Loading editor scene", this);
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
            AppLog.LogInfo("Unloading editor scene", this);
            await SceneManager.UnloadSceneAsync(editorScene);
        }

        EventManager.NotifyEditorUnloaded();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AppLog.LogTrace($"Scene {scene.name} was loaded", this);
        if (BrandManager.Instance.Customizable)
        {
            BrandManager.Instance.AddCustomColors();
        }

        if (scene.name == PlatformManager.Instance.PlayerSceneName)
        {
            editorScene = scene;
        }
    }
}
