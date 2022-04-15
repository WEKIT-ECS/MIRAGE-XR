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
        ToggleObject myObj;
        [SerializeField] private Transform pickOb;
        [SerializeField] private Transform targetOb;
        [SerializeField] private Transform lockToggle;
        [SerializeField] private SpriteToggle st;
        [SerializeField] private Text textLabel;
        Pick place;

        private Vector3 defaultTargetSize = new Vector3(0.2f, 0.2f, 0.2f);

        private async void Start()
        {
            place = pickOb.GetComponent<Pick>();
            EditModeChanges(ActivityManager.Instance.EditModeActive);
            await Load();

            st.IsSelected = !place.MoveMode;
            
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
            get {
                return targetOb;
            }
        }

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
            lockToggle.gameObject.SetActive(EditModeState);
            targetOb.gameObject.SetActive(EditModeState);
            place.ChangeModelButton.gameObject.SetActive(EditModeState);
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
            transform.parent.localScale = GetPoiScale(myPoiEditor, defaultScale);

            // If everything was ok, return base result.
            return base.Init(obj);
        }


        private async Task<bool> Load()
        {
            var loaded = await LoadPositions(Path.Combine(ActivityManager.Instance.Path, "pickandplaceinfo/" + myObj.poi + ".json"));
            return loaded;
        }


        public void SavePositions() {

            if (myObj == null || myObj.poi == "") return;//only if the poi is instantiated not the prefab

            Positions positions = new Positions
            {
                pickObPos = pickOb.localPosition,
                PickObRot = pickOb.localRotation,
                modelID = place.MyModelID,
                targetObPos = targetOb.localPosition,
                targetObScale = targetOb.localScale,
                resetPos = place.resetPos,
                moveMode = place.MoveMode
            };

            string pandpjson = JsonUtility.ToJson(positions);
            if (!Directory.Exists(ActivityManager.Instance.Path + "/pickandplaceinfo "))
                Directory.CreateDirectory(ActivityManager.Instance.Path + "/pickandplaceinfo");


            string jsonPath = Path.Combine(ActivityManager.Instance.Path, "pickandplaceinfo/" + myObj.poi + ".json");

            //delete the exsiting file first
            if (File.Exists(jsonPath))
                File.Delete(jsonPath);

            File.WriteAllText(jsonPath, pandpjson);
        }

        public async Task<bool> LoadPositions(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return false;

            Positions positions = JsonUtility.FromJson<Positions>(File.ReadAllText(jsonPath));

            await Task.Delay(1);

            pickOb.localPosition = positions.pickObPos;
            pickOb.localRotation = positions.PickObRot;
            targetOb.localPosition = positions.targetObPos;
            targetOb.localScale = positions.targetObScale != null ? positions.targetObScale : defaultTargetSize;
            place.MoveMode = positions.moveMode;
            place.resetPos = positions.resetPos;
            place.MyModelID = positions.modelID;

            if (place.MyModelID != "")
                StartCoroutine(LoadMyModel(place.MyModelID));

            return true;
        }

        IEnumerator LoadMyModel(string MyModelID)
        {
            var newModel = GameObject.Find(MyModelID);

            //wait until all model are loaded
            while (newModel == null)
            {
                newModel = GameObject.Find(MyModelID);
                yield return null;
            }

            StartCoroutine(ActionEditor.Instance.SpwanNewPickModel(place, newModel));
        }


    }

    [Serializable]
    public class Positions
    {
        public Vector3 pickObPos = Vector3.zero;
        public Quaternion PickObRot = Quaternion.identity;
        public Vector3 targetObPos = Vector3.zero;
        public Vector3 targetObScale = Vector3.zero;

        public Vector3 resetPos = Vector3.zero;
        public bool moveMode = false;
        public string modelID;
    }
}
