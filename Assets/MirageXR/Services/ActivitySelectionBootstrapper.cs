using i5.Toolkit.Core.ServiceCore;
using UnityEngine;

public class ActivitySelectionBootstrapper : BaseServiceBootstrapper
{
    [SerializeField] private ActivitySelectionSceneReferenceServiceConfiguration referenceServiceConfiguration;

    protected override void RegisterServices()
    {
        ActivitySelectionSceneReferenceService referenceService = new ActivitySelectionSceneReferenceService(referenceServiceConfiguration);
        ServiceManager.RegisterService(referenceService);
    }

    protected override void UnRegisterServices()
    {
        if (ServiceManager.ServiceExists<ActivitySelectionSceneReferenceService>())
        {
            ServiceManager.RemoveService<ActivitySelectionSceneReferenceService>();
        }
    }
}
