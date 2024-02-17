using UnityEngine;

namespace Obi
{
    [RequireComponent(typeof(ObiActor))]
    public class SetCategory : MonoBehaviour
    {
        public int category;
        private ObiActor act;

        private void Awake()
        {
            act = GetComponent<ObiActor>();
            act.OnBlueprintLoaded += OnLoad;

            if (act.isLoaded)
				act.SetFilterCategory(category);
		}

        private void OnDestroy()
        {
            act.OnBlueprintLoaded -= OnLoad;
        }

        private void OnValidate()
        {
            category = Mathf.Clamp(category, ObiUtils.MinCategory, ObiUtils.MaxCategory);

            if (act != null && act.isLoaded)
				act.SetFilterCategory(category);
		}

        private void OnLoad(ObiActor actor, ObiActorBlueprint blueprint)
        {
            actor.SetFilterCategory(category);
        }
    }
}
