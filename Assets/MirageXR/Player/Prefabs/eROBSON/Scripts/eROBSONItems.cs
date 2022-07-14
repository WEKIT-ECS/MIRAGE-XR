using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;


namespace MirageXR
{
    public class eROBSONItems : MirageXRPrefab
    {

        private static ActivityManager activityManager => RootObject.Instance.activityManager;
        private ToggleObject myObj;

        [Tooltip("The bit id")]
        [SerializeField] private string id;
        public string ID => id;

        private Port[] ports;
        public Port[] Ports => ports;

        public bool IsMoving { get; private set; }


        private void OnEnable()
        {
            EventManager.OnEditModeChanged += SetEditorState;
        }

        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= SetEditorState;
        }

        private void Start()
        {
            SetEditorState(activityManager.EditModeActive);
            ports = GetComponentsInChildren<Port>();
            gameObject.GetComponentInParent<ObjectManipulator>().OnManipulationStarted.AddListener(delegate { IsMoving = true; });
            gameObject.GetComponentInParent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { IsMoving = false; });
        }

        public void DisableManipulation()
        {
            gameObject.GetComponentInParent<ObjectManipulator>().enabled = false;
        }

        public void EnableManipulation()
        {
            gameObject.GetComponentInParent<ObjectManipulator>().enabled = true;
        }


        private void SetEditorState(bool editModeActive)
        {

            var boundsControl = GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.Active = editModeActive;
            }
        }

        public override bool Init(ToggleObject obj)
        {
            myObj = obj;

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;
            obj.text = name;

            // Set scaling if defined in action configuration.
            var myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
            transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);

            // If everything was ok, return base result.
            return base.Init(obj);
        }
    }
}