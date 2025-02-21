using LearningExperienceEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class PickAndPlaceController : MirageXRPrefab
    {
        private static LearningExperienceEngine.ActivityManager _activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

        private LearningExperienceEngine.ToggleObject _myObj;
        [SerializeField] private Transform _pickObject;
        [SerializeField] private Transform _targetObject;
        [SerializeField] private Text _textLabel;
        private Pick _pickComponent;


        private Vector3 _defaultTargetSize = new Vector3(0.2f, 0.2f, 0.2f);

        public LearningExperienceEngine.ToggleObject MyPoi => _myObj;

        public Transform PickObject => _pickObject;

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
            LearningExperienceEngine.EventManager.OnAugmentationDeleted += DeletePickAndPlaceData;
            LearningExperienceEngine.EventManager.OnActivitySaved += OnActivitySaved;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
            LearningExperienceEngine.EventManager.OnAugmentationDeleted -= DeletePickAndPlaceData;
            LearningExperienceEngine.EventManager.OnActivitySaved -= OnActivitySaved;
        }

        private void EditModeChanges(bool editModeState)
        {
            _targetObject.gameObject.SetActive(editModeState);
            _pickComponent.ChangeModelButton.gameObject.SetActive(editModeState);
            _pickComponent.SetMoveMode(editModeState);
            var boundsControl = _pickObject.GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.Active = editModeState;
            }

            if (!editModeState)
            {
                SavePositions();
            }
        }

        private void OnEditModeChanged(bool editModeState)
        {
            EditModeChanges(editModeState);

            if (!editModeState)
            {
                LearningExperienceEngine.EventManager.ActivitySaved();
            }
        }

        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            _myObj = obj;

            _textLabel.text = _myObj.text;

            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;

            if (!base.Init(obj))
            {
                return false;
            }

            var manipulator = GetComponentInParent<ObjectManipulator>();
            if (manipulator)
            {
                manipulator.enabled = false;
            }

            _pickComponent = _pickObject.GetComponent<Pick>();

            if (File.Exists(Path.Combine(_activityManager.ActivityPath, $"pickandplaceinfo/{_myObj.poi}.json")))
            {
                LoadPickAndPlacePositions();
            }

            EditModeChanges(_activityManager.EditModeActive);

            CheckTrigger("correct");
            CheckTrigger("incorrect");

            return true;
        }

        private void LoadPickAndPlacePositions()
        {
            var json = File.ReadAllText(Path.Combine(_activityManager.ActivityPath, $"pickandplaceinfo/{_myObj.poi}.json"));
            var positions = JsonUtility.FromJson<Positions>(json);

            _pickObject.localPosition = _myObj.key == "1" ? positions.resetPosition : positions.pickObjectPosition;
            _pickObject.localRotation = positions.pickObjectRotation;
            _pickObject.localScale = positions.pickObjectScale;

            _targetObject.localPosition = positions.targetObjectPosition;
            _targetObject.localScale = positions.targetObjectScale != Vector3.zero ? positions.targetObjectScale : _defaultTargetSize;

            _pickComponent.MoveMode = positions.moveMode;
            _pickComponent.ResetPosition = positions.resetPosition;
            _pickComponent.ResetRotation = positions.resetRotation;
            _pickComponent.MyModelID = positions.modelID;

            if (_pickComponent.MyModelID != string.Empty)
            {
                StartCoroutine(LoadMyModel(_pickComponent.MyModelID));
            }
        }

        private void OnActivitySaved()
        {
            if (LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive)
            {
                SavePositions();
            }
        }

        private void SavePositions()
        {
            if (_myObj == null || _myObj.poi == string.Empty || gameObject == null)
            {
                return; // only if the poi is instantiated not the prefab
            }

            try
            {
                var positions = new Positions
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

                var pickAndPlaceData = JsonUtility.ToJson(positions);
                if (!Directory.Exists($"{_activityManager.ActivityPath}/pickandplaceinfo"))
                {
                    Directory.CreateDirectory($"{_activityManager.ActivityPath}/pickandplaceinfo");
                }

                var jsonPath = Path.Combine(_activityManager.ActivityPath, $"pickandplaceinfo/{_myObj.poi}.json");

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

            //wait for model component to be added
            while (!newModel.GetComponentInChildren<Model>())
            {
                yield return null;
            }

            StartCoroutine(ActionEditor.Instance.SpawnNewPickModel(_pickComponent, newModel));
        }

        private void DeletePickAndPlaceData(LearningExperienceEngine.ToggleObject toggleObject)
        {
            if (toggleObject != MyPoi)
            {
                return;
            }

            var arlemPath = _activityManager.ActivityPath;
            var jsonPath = Path.Combine(arlemPath, $"pickandplaceinfo/{MyPoi.poi}.json");

            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
        }

        private void OnDestroy()
        {
            if (LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive)
            {
                SavePositions();
            }
        }

        private void CheckTrigger(string suffix)
        {
            if (Annotation != null)
            {
                var trigger = _activityManager.ActiveAction.triggers.Find(t => t.id == Annotation.poi + suffix);
                var _isTrigger = trigger != null ? true : false;

                if (_isTrigger)
                {
                    _pickComponent.SetTrigger(trigger);
                }
            }
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
