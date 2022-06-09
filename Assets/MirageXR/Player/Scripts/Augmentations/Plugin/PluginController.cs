using UnityEngine;

namespace MirageXR
{
    public class PluginController : MirageXRPrefab
    {
        private GameObject plugin;
        [SerializeField] private Transform axis;

        private void OnEnable()
        {
            EventManager.OnEditModeChanged += EditModeChanges;
        }

        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= EditModeChanges;
        }

        private void EditModeChanges(bool EditModeState)
        {
            axis.gameObject.SetActive(EditModeState);
        }

        public override bool Init(ToggleObject obj)
        {
            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            loadPlugin(obj.url);

            // Set scaling
            gameObject.name = obj.predicate;
            PoiEditor myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
            Vector3 defaultScale = new Vector3(0.5f, 0.5f, 0.5f);
            transform.parent.localScale = GetPoiScale(myPoiEditor, defaultScale);

            // If everything was ok, return base result.
            return base.Init(obj);
        }


        private void loadPlugin(string path)
        {

            Debug.Log(path);
            plugin = Instantiate(Resources.Load<GameObject>(path), Vector3.zero, Quaternion.identity);

            plugin.transform.parent = gameObject.transform; // pluginParent;
            plugin.transform.localPosition = new Vector3(0, 1, 0);
        }
    }
}
