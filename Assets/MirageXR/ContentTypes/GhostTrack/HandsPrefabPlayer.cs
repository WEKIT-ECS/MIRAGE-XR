using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class HandsPrefabPlayer : MirageXRPrefab
    {

        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            // Set name. Please don't modify so that the ghosttrack can be deactivated...
            name = obj.predicate;

            // IMPLEMENT THE ACTUAL INITIALIZATION STARTING FROM HERE. Check various prefab scripts for examples.

            return true;
        }
    }
}