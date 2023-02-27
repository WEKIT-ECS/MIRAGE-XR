using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;


namespace MirageXR
{
    public class PickAndPlaceController : MirageXRPrefab
    {
        private static ActivityManager _activityManager => RootObject.Instance.activityManager;

        private ToggleObject _myObj;
        [SerializeField] private Transform _pickObject;
        [SerializeField] private Transform _targetObject;
        [SerializeField] private Transform _lockToggle;
        [SerializeField] private SpriteToggle _spriteToggle;
        [SerializeField] private Text _textLabel;
        private Pick _pickComponent;

        private bool _isTrigger;

        private Vector3 _defaultTargetSize = new Vector3(0.2f, 0.2f, 0.2f);

        private void Start()
        {
            _pickComponent = _pickObject.GetComponent<Pick>();
            EditModeChanges(_activityManager.EditModeActive);

            if (File.Exists(Path.Combine(_activityManager.ActivityPath, "pickandplaceinfo/" + _myObj.poi + ".json")))
            {
                LoadPickAndPlacePositions();
            }

            _spriteToggle.IsSelected = !_pickComponent.MoveMode;
            CheckTrigger();
        }

        public ToggleObject MyPoi
        {
            get
            {
                return _myObj;
            }
        }

        public Transform Target
        {
            get
            {
                return _targetObject;
            }
        } 
        
        public Transform PickObject
        {
            get
            {
                return _pickObject;
            }
        }

        private void OnEnable()
        {
            EventManager.OnEditModeChanged += EditModeChanges;
            EventManager.OnAugmentationDeleted += DeletePickAndPlaceData;
            EventManager.OnActivitySaved += SavePositions;
        }

        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= EditModeChanges;
            EventManager.OnAugmentationDeleted -= DeletePickAndPlaceData;
            EventManager.OnActivitySaved -= SavePositions;
        }

        private void EditModeChanges(bool editModeState)
        {
            _lockToggle.gameObject.SetActive(editModeState);
            _targetObject.gameObject.SetActive(editModeState);
            _pickComponent.ChangeModelButton.gameObject.SetActive(editModeState);
            _pickComponent.EditMode = editModeState;
            var boundsControl = _pickObject.GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.Active = editModeState;
            }
        }

        public override bool Init(ToggleObject obj)
        {
            _myObj = obj;

            _textLabel.text = _myObj.text;

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;

            // Set scaling
            PoiEditor myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
            Vector3 defaultScale = new Vector3(0.5f, 0.5f, 0.5f);

            // If everything was ok, return base result.
            return base.Init(obj);
        }

        private void LoadPickAndPlacePositions()
        {
            var json = File.ReadAllText(Path.Combine(_activityManager.ActivityPath, "pickandplaceinfo/" + _myObj.poi + ".json"));

            Positions positions = JsonUtility.FromJson<Positions>(json);

            if (_myObj.key == "1")
            {
                _pickObject.localPosition = positions.resetPosition;
            }
            else
            {
                _pickObject.localPosition = positions.pickObjectPosition;
            }

            _pickObject.localRotation = positions.pickObjectRotation;
            _pickObject.localScale = positions.pickObjectScale;
            _targetObject.localPosition = positions.targetObjectPosition;
            _targetObject.localScale = positions.targetObjectScale != null ? positions.targetObjectScale : _defaultTargetSize;
            _pickComponent.MoveMode = positions.moveMode;
            _pickComponent.ResetPosition = positions.resetPosition;
            _pickComponent.ResetRotation = positions.resetRotation;
            _pickComponent.MyModelID = positions.modelID;

            if (_pickComponent.MyModelID != string.Empty)
                StartCoroutine(LoadMyModel(_pickComponent.MyModelID));
        }

        public void SavePositions()
        {
            if (_myObj == null || _myObj.poi == string.Empty || gameObject == null)
            {
                return; // only if the poi is instantiated not the prefab
            }
            try
            {
                Positions positions = new Positions
                {
                    pickObjectPosition = _pickObject.localPosition,
                    pickObjectRotation = _pickObject.localRotation,
                    pickObjectScale = _pickObject.localScale,
                    modelID = _pickComponent.MyModelID,
                    targetObjectPosition = _targetObject.localPosition,
                    targetObjectScale = _targetObject.localScale,
                    resetPosition = _pickComponent.ResetPosition,
                    resetRotation = _pickComponent.ResetRotation,
                    moveMode = _pickComponent.MoveMode,
                };

                string pickAndPlaceData = JsonUtility.ToJson(positions);
                if (!Directory.Exists($"{_activityManager.ActivityPath}/pickandplaceinfo"))
                {
                    Directory.CreateDirectory($"{_activityManager.ActivityPath}/pickandplaceinfo");
                }

                string jsonPath = Path.Combine(_activityManager.ActivityPath, $"pickandplaceinfo/{_myObj.poi}.json");

                // delete the exsiting file first
                if (File.Exists(jsonPath))
                {
                    File.Delete(jsonPath);
                }

                File.WriteAllText(jsonPath, pickAndPlaceData);
            }
            catch (Exception e) 
            {
                Debug.LogError("Pick and Place Exception " + e); 
            }
        }

        private IEnumerator LoadMyModel(string MyModelID)
        {
            var newModel = GameObject.Find(MyModelID);

            // wait until all model are loaded
            while (newModel == null)
            {
                newModel = GameObject.Find(MyModelID);
                yield return null;
            }

            StartCoroutine(ActionEditor.Instance.SpawnNewPickModel(_pickComponent, newModel));
        }

        private void DeletePickAndPlaceData(ToggleObject toggleObject)
        {
            if (toggleObject != MyPoi) return;
            var arlemPath = _activityManager.ActivityPath;
            var jsonPath = Path.Combine(arlemPath, $"pickandplaceinfo/{MyPoi.poi}.json");

            if (File.Exists(jsonPath))
            {
                // delete the json
                File.Delete(jsonPath);
            }
        }

        private void OnDestroy()
        {
            SavePositions();
        }

        private void CheckTrigger()
        {
            var trigger = _activityManager.ActiveAction.triggers.Find(t => t.id == Annotation.poi);
            _isTrigger = trigger != null ? true : false;
            _pickComponent.SetTrigger(trigger);
        }
    }

    [Serializable]
    public class Positions
    {
        public Vector3 pickObjectPosition = Vector3.zero;
        public Quaternion pickObjectRotation = Quaternion.identity;
        public Vector3 pickObjectScale = Vector3.zero;
        public Vector3 targetObjectPosition = Vector3.zero;
        public Vector3 targetObjectScale = Vector3.zero;

        public Vector3 resetPosition = Vector3.zero;
        public Quaternion resetRotation = Quaternion.identity;
        public bool moveMode = false;
        public bool reset = false;
        public string modelID;
    }
}
