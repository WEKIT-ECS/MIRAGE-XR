using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;


namespace MirageXR
{
    public class PickAndPlaceController : MirageXRPrefab
    {
        private ToggleObject myObj;
        [SerializeField] private Transform pickObject;
        [SerializeField] private Transform targetObject;
        [SerializeField] private Transform lockToggle;
        [SerializeField] private SpriteToggle spriteToggle;
        [SerializeField] private Text textLabel;
        private Pick pickComponent;

        private Vector3 defaultTargetSize = new Vector3(0.2f, 0.2f, 0.2f);

        private  void Start()
        {
            pickComponent = pickObject.GetComponent<Pick>();
            EditModeChanges(ActivityManager.Instance.EditModeActive);

            LoadPickAndPlacePositions();
            
            spriteToggle.IsSelected = !pickComponent.MoveMode;

        }

        public ToggleObject MyPoi
        {
            get
            {
                return myObj;
            }
        }

        public Transform Target
        {
            get
            {
                return targetObject;
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
            lockToggle.gameObject.SetActive(editModeState);
            targetObject.gameObject.SetActive(editModeState);
            pickComponent.ChangeModelButton.gameObject.SetActive(editModeState);
        }


        public override bool Init(ToggleObject obj)
        {
            myObj = obj;

            textLabel.text = myObj.text;

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
            var json = File.ReadAllText(Path.Combine(ActivityManager.Instance.Path, "pickandplaceinfo/" + myObj.poi + ".json"));

            Positions positions = JsonUtility.FromJson<Positions>(json);

            if (myObj.key == "1")
            {
                pickObject.localPosition = positions.resetPosition;
            }
            else
            {
                pickObject.localPosition = positions.pickObjectPosition;
            }

            pickObject.localRotation = positions.pickObjectRotation;
            targetObject.localPosition = positions.targetObjectPosition;
            targetObject.localScale = positions.targetObjectScale != null ? positions.targetObjectScale : defaultTargetSize;
            pickComponent.MoveMode = positions.moveMode;
            pickComponent.ResetPos = positions.resetPosition;
            pickComponent.MyModelID = positions.modelID;

            if (pickComponent.MyModelID != string.Empty)
                StartCoroutine(LoadMyModel(pickComponent.MyModelID));
        }

        public void SavePositions()
        {

            if (myObj == null || myObj.poi == "") return;//only if the poi is instantiated not the prefab

            Positions positions = new Positions
            {
                pickObjectPosition = pickObject.localPosition,
                pickObjectRotation = pickObject.localRotation,
                modelID = pickComponent.MyModelID,
                targetObjectPosition = targetObject.localPosition,
                targetObjectScale = targetObject.localScale,
                resetPosition = pickComponent.ResetPos,
                moveMode = pickComponent.MoveMode
            };

            string pickAndPlaceData = JsonUtility.ToJson(positions);
            if (!Directory.Exists(ActivityManager.Instance.Path + "/pickandplaceinfo "))
                Directory.CreateDirectory(ActivityManager.Instance.Path + "/pickandplaceinfo");


            string jsonPath = Path.Combine(ActivityManager.Instance.Path, "pickandplaceinfo/" + myObj.poi + ".json");

            //delete the exsiting file first
            if (File.Exists(jsonPath))
                File.Delete(jsonPath);

            File.WriteAllText(jsonPath, pickAndPlaceData);
        }
     

        private IEnumerator LoadMyModel(string MyModelID)
        {
            var newModel = GameObject.Find(MyModelID);

            //wait until all model are loaded
            while (newModel == null)
            {
                newModel = GameObject.Find(MyModelID);
                yield return null;
            }

            StartCoroutine(ActionEditor.Instance.SpawnNewPickModel(pickComponent, newModel));
        }

        private void DeletePickAndPlaceData(ToggleObject toggleObject)
        {
            if (toggleObject != MyPoi) return;
            var arlemPath = ActivityManager.Instance.Path;
            var jsonPath = Path.Combine(arlemPath, $"pickandplaceinfo/{MyPoi.poi}.json");

            if (File.Exists(jsonPath))
            {
                //delete the json
                File.Delete(jsonPath);
            }
        }

        private void OnDestroy()
        {
            SavePositions();
        }

    }



    [Serializable]
    public class Positions
    {
        public Vector3 pickObjectPosition = Vector3.zero;
        public Quaternion pickObjectRotation = Quaternion.identity;
        public Vector3 targetObjectPosition = Vector3.zero;
        public Vector3 targetObjectScale = Vector3.zero;

        public Vector3 resetPosition = Vector3.zero;
        public bool moveMode = false;
        public bool reset = false;
        public string modelID;
    }
}
