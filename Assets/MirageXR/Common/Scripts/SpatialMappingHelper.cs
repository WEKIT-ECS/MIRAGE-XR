using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MirageXR
{
    public class SpatialMappingHelper
    {
        public static void ActivateSpatialMapping()
        {
            Microsoft.MixedReality.Toolkit.CoreServices.SpatialAwarenessSystem?.ResumeObservers();
            SetSpatialMappingVisibility(SpatialAwarenessMeshDisplayOptions.Visible);
        }

        public static void DeactivateSpatialMapping()
        {
            SetSpatialMappingVisibility(SpatialAwarenessMeshDisplayOptions.None);
            Microsoft.MixedReality.Toolkit.CoreServices.SpatialAwarenessSystem?.SuspendObservers();
        }

        private static void SetSpatialMappingVisibility(SpatialAwarenessMeshDisplayOptions option)
        {
            if (Microsoft.MixedReality.Toolkit.CoreServices.SpatialAwarenessSystem is IMixedRealityDataProviderAccess provider)
            {
                foreach (var observer in provider.GetDataProviders())
                {
                    if (observer is IMixedRealitySpatialAwarenessMeshObserver meshObs)
                    {
                        meshObs.DisplayOption = option;
                    }
                }
            }
        }
    }
}