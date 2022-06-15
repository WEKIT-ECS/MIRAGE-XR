using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using System.Collections;

namespace MirageXR
{
    public class eROBSONItems : MirageXRPrefab
    {
        private static ActivityManager activityManager => RootObject.Instance.activityManager;
        private ToggleObject myObj;

        public GameObject TouchedObject { get; private set; }

        [Tooltip("The bit id")]
        [SerializeField] private string id;
        public string ID => id;

        private Port[] ports;
        public Port[] Ports => ports;


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
            gameObject.GetComponentInParent<ObjectManipulator>().OnManipulationStarted.AddListener(delegate { TouchedObject = gameObject; });
            gameObject.GetComponentInParent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { StartCoroutine(DoAfterDelay()); });

            ports = GetComponentsInChildren<Port>();
        }

        IEnumerator DoAfterDelay()
        {
            TouchedObject = null;
            yield return new WaitForSeconds(1);
            gameObject.GetComponentInParent<ObjectManipulator>().enabled = true;
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