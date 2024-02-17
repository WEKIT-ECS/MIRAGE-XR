using i5.Toolkit.Core.ServiceCore;

public class ActivitySelectionSceneReferenceService : IService
{
    public ActivitySelectionSceneReferenceServiceConfiguration References { get; private set; }

    public ActivitySelectionSceneReferenceService(ActivitySelectionSceneReferenceServiceConfiguration configuration)
    {
        References = configuration;
    }

    public void Cleanup()
    {
    }

    public void Initialize(IServiceManager owner)
    {
    }
}
