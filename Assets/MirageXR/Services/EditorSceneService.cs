using System.Threading.Tasks;
using MirageXR;

public class EditorSceneService
{
    public Task LoadEditorAsync()
    {
        EventManager.NotifyEditorLoaded();
        return Task.CompletedTask;
    }

    public Task UnloadExistingScene()
    {
        EventManager.NotifyEditorUnloaded();
        return Task.CompletedTask;
    }
}
