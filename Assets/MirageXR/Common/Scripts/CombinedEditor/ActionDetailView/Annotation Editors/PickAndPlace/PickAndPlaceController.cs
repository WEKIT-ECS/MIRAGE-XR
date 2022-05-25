using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace MirageXR
{
    public class PickAndPlaceController : MirageXRPrefab
    {
        private static ActivityManager activityManager => RootObject.Instance.activityManager;
        [SerializeField] private Transform pickOb;
        [SerializeField] private Transform targetOb;
        [SerializeField] private Transform lockToggle;
        [SerializeField] private SpriteToggle st;
        [SerializeField] private Text textLabel;
        
        private Pick place;
        private ToggleObject _myObj;

        public ToggleObject MyPoi => _myObj;
        public Transform Target => targetOb;

        private void Start()
        {
            Subscribe();
            place = pickOb.GetComponent<Pick>();
            EditModeChanges(activityManager.EditModeActive);
            Load();

            st.IsSelected = !place.MoveMode;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }


        private void Subscribe()
        {
            EventManager.OnEditModeChanged += EditModeChanges;
            EventManager.OnAugmentationDeleted += DeletePickAndPlaceData;
            EventManager.OnActivitySaved += SavePositions;
            EventManager.OnToggleObject += OnToggleObjectActivated;
        }

        private void Unsubscribe()
        {
            EventManager.OnEditModeChanged -= EditModeChanges;
            EventManager.OnAugmentationDeleted -= DeletePickAndPlaceData;
            EventManager.OnActivitySaved -= SavePositions;
            EventManager.OnToggleObject -= OnToggleObjectActivated;
        }

        private void EditModeChanges(bool editModeState)
        {
            lockToggle.gameObject.SetActive(editModeState);
            targetOb.gameObject.SetActive(editModeState);
            place.ChangeModelButton.gameObject.SetActive(editModeState);
        }

        public override bool Init(ToggleObject obj)
        {
            _myObj = obj;

            textLabel.text = _myObj.text;

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
            transform.parent.localScale = GetPoiScale(myPoiEditor, defaultScale);

            // If everything was ok, return base result.
            return base.Init(obj);
        }

        private bool Load()
        {
            return LoadPositions(Path.Combine(activityManager.ActivityPath, "pickandplaceinfo/" + _myObj.poi + ".json"));
        }

        private void OnToggleObjectActivated(ToggleObject toggleObject, bool value)
        {
            if (!value && _myObj.poi == toggleObject.poi)
            {
                SavePositions();
            }
        }
        
        public void SavePositions() 
        {
            if (_myObj == null || _myObj.poi == string.Empty) return; //only if the poi is instantiated not the prefab

            var positions = new Positions
            {
                pickObPos = pickOb.localPosition,
                pickObRot = pickOb.localRotation,
                modelID = place.MyModelID,
                targetObPos = targetOb.localPosition,
                targetObScale = targetOb.localScale,
                resetPos = place.ResetPos,
                moveMode = place.MoveMode
            };

            var json = JsonUtility.ToJson(positions);
            if (!Directory.Exists(activityManager.ActivityPath + "/pickandplaceinfo"))
            {
                Directory.CreateDirectory(activityManager.ActivityPath + "/pickandplaceinfo");
            }

            var jsonPath = Path.Combine(activityManager.ActivityPath, $"pickandplaceinfo/{_myObj.poi}.json");

            //delete the existing file first
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }

            File.WriteAllText(jsonPath, json);
        }

        public bool LoadPositions(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return false;

            var positions = JsonUtility.FromJson<Positions>(File.ReadAllText(jsonPath));

            pickOb.localPosition = positions.pickObPos;
            pickOb.localRotation = positions.pickObRot;
            targetOb.localPosition = positions.targetObPos;
            targetOb.localScale = positions.targetObScale;
            place.MoveMode = positions.moveMode;
            place.ResetPos = positions.resetPos;
            place.MyModelID = positions.modelID;

            if (place.MyModelID != string.Empty)
            {
                StartCoroutine(LoadMyModel(place.MyModelID));
            }

            return true;
        }

        private IEnumerator LoadMyModel(string myModelID)
        {
            var newModel = GameObject.Find(myModelID);

            //wait until all model are loaded
            while (newModel == null)
            {
                newModel = GameObject.Find(myModelID);
                yield return null;
            }

            StartCoroutine(ActionEditor.Instance.SpawnNewPickModel(place, newModel));
        }

        private void DeletePickAndPlaceData(ToggleObject toggleObject)
        {
            if (toggleObject != MyPoi) return;
            var arlemPath = activityManager.ActivityPath;
            var jsonPath = Path.Combine(arlemPath, $"pickandplaceinfo/{MyPoi.poi}.json");

            if (File.Exists(jsonPath))
            {
                //delete the json
                File.Delete(jsonPath);
            }
        }
    }

    [Serializable]
    public class Positions
    {
        public Vector3 pickObPos = Vector3.zero;
        public Quaternion pickObRot = Quaternion.identity;
        public Vector3 targetObPos = Vector3.zero;
        public Vector3 targetObScale = Vector3.zero;
        public Vector3 resetPos = Vector3.zero;
        public bool moveMode;
        public string modelID;
    }
}
