using i5.Toolkit.Core.ServiceCore;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;
using MirageXR;
using Vuforia;
using UnityEngine.XR.ARFoundation;
public class EditorSceneService : IService
{
    private const string recorderSceneName = "Recorder";

    private Scene recorderScene;
    private Scene editorScene;

    public void Initialize(IServiceManager owner)
    {
    }

    public void Cleanup()
    {
    }

    public async Task LoadEditorAsync()
    {
        await UnloadExistingScene();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        await SceneManager.LoadSceneAsync(PlatformManager.Instance.PlayerSceneName, LoadSceneMode.Additive);
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

        EventManager.NotifyEditorLoaded();
    }

    public async Task LoadRecorderAsync()
    {
        await UnloadExistingScene();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        await SceneManager.LoadSceneAsync(recorderSceneName, LoadSceneMode.Additive);
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

        EventManager.NotifyEditorLoaded();
    }

    public async Task UnloadExistingScene()
    {

        VuforiaBehaviour.Instance.enabled = false;

        if (editorScene != null && editorScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(editorScene);
        }
        if (recorderScene != null && recorderScene.isLoaded)
        {
            await SceneManager.UnloadSceneAsync(recorderScene);
        }

        EventManager.NotifyEditorUnloaded();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(BrandManager.Instance.Customizable)
            BrandManager.Instance.AddCustomColors();

        if (scene.name == PlatformManager.Instance.PlayerSceneName)
        {
            editorScene = scene;
        }
        else if (scene.name == recorderSceneName)
        {
            recorderScene = scene;
        }

    }

}
