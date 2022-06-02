using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MirageXR
{
    public class TaskStationController : MonoBehaviour, IMixedRealityFocusHandler
    {
        #region Variables

        [SerializeField] private Renderer taskStationRenderer;

        public System.Guid TaskStationId { get; private set; }
        public string TaskStationName { get; private set; }
        // private TaskStationContextMenu myTSCM;

        public bool IsMenuOpen { get; private set; }

        // arlem related objects
        public Action ThisAction { get; private set; }
        public int ThisActionIndex { get; private set; }
        private int trigIndex;

        // workplace objects
        public Place thisPlace { get; private set; }
        public bool hasMarker = false;
        private System.Guid markerId;
        public Detectable thisDetectable { get; private set; }

        public int thisPlaceIndex { get; private set; }
        public int thisDetectIndex { get; private set; }

        public List<GameObject> annotationList { get; private set; } = new List<GameObject>();
        private GameObject currentAnnotation;

        [HideInInspector]
        public GameObject highlighter { get; private set; }

        public int ArlemAnnotationCounter { get; private set; }

        private Color startColour;

        #endregion Variables


        [Header("Gaze-focus Settings")]
        [Tooltip("Color used when object is gazed at.")]
        public Color FocusColor = Color.yellow;

        [Tooltip("Factor at which the object should be enlarged on gaze")]
        [Range(0.8f, 2.0f)]
        public float FocusSizeFactor = 1.2f;


        [Header("Menu Animatation Settings")]
        [Tooltip("Radial distance from object")]
        public float menuItemDistance = 0.15f; // 

        [Tooltip("rate of menu expansion/contraction")]
        public float menuSpeed = 0.5f; // 


        private void Awake()
        {
            // myTSCM = gameObject.GetComponent<TaskStationContextMenu>();

            // name and link gameobject to arlem data model
            TaskStationId = System.Guid.NewGuid();
            TaskStationName = ("TS-" + TaskStationId.ToString());

            // specify action values
            ThisAction = ServiceManager.GetService<ActivityRecorderService>().myActivity.actions.Last();
            ThisAction.id = TaskStationName;
            ThisAction.viewport = "actions";
            ThisAction.type = "action";
            ThisAction.instruction = new Instruction();
            ThisAction.enter = new Enter();
            ThisAction.exit = new Exit();
            ThisAction.triggers = new List<Trigger>();

            // thisAction.instruction.title = "Action Step " + thisActionIndex.ToString();
            // thisAction.instruction.description = "Add task step description here...";

            // specify workplace values
            thisPlace = ServiceManager.GetService<ActivityRecorderService>().myWorkplace.places.Last();
            thisDetectable = ServiceManager.GetService<ActivityRecorderService>().myWorkplace.detectables.Last();

            // add a default poi to the place
            /*
            Poi defaultPoi = new Poi();
            defaultPoi.id = "default";
            defaultPoi.offset = "0, 0, 0";
            thisPlace.pois.Add(defaultPoi);
            */

        }

        public void Start()
        {
            ArlemAnnotationCounter = 0;

            // get colour set in unity
            startColour = taskStationRenderer.material.color;

            // give the task station a world anchor
            ServiceManager.GetService<WorldAnchorService>().Manager.AttachAnchor(this.gameObject, "WA-" + TaskStationId);

            // add trigger and activate TS
            AddArlemTrigger("voice");
            AddArlemTrigger("click");

            // give initial values for arlem model
            UpdateActionInfo();
            UpdateWorkplaceInfo();

            ActivateTaskStation();
        }


        #region ARLEM Data Handling


        private void CreateArlemAnnotation()
        {
            if (ThisAction == null) { return; }

            // make new enter object, not using deactivates on these
            ThisAction.enter.removeSelf = false;
            ThisAction.enter.messages.Add(new Message());
            ThisAction.enter.activates.Add(new Activate());

            // also create an exit object (w/out activate options)
            ThisAction.exit.removeSelf = true;
            ThisAction.exit.messages.Add(new Message());
            ThisAction.exit.deactivates.Add(new Deactivate());

            thisPlace.pois.Add(new Poi());
        }


        private void AddArlemTrigger(string mode)
        {
            // add a trigger, give it an id
            ThisAction.triggers.Add(new Trigger());
            trigIndex = ThisAction.triggers.Count - 1;
            ThisAction.triggers[trigIndex].id = "start"; // ("TR-" + System.Guid.NewGuid().ToString());

            // need a method to make this accessable to the user
            ThisAction.triggers[trigIndex].mode = mode;
            ThisAction.triggers[trigIndex].type = "action";
            ThisAction.triggers[trigIndex].viewport = "actions";
            ThisAction.triggers[trigIndex].duration = 0f;
        }

        private void UpdateActionInfo() // called whenever TS is deactivated
        {
            // reference ARLEM model - loads changes to the local variables
            List<Action> currentActionList = ServiceManager.GetService<ActivityRecorderService>().myActivity.actions;
            foreach (Action action in currentActionList.Where(a => a.id == TaskStationName))
            {
                ThisAction = action;
                ThisActionIndex = currentActionList.IndexOf(action);
            }

            
            ThisAction.viewport = "actions";
            ThisAction.type = "action";
            ThisAction.device = "wekit.one";
            ThisAction.location = "here";
            ThisAction.predicate = "none";
            ThisAction.user = "";

            ThisAction.instruction.title = "Action Step " + (ThisActionIndex + 1).ToString(); // Fudge fix: Added +1 to remove "Action Step 0". Need more information if we want to increase thisActionIndex by 1 (Might be it needs the 0)
            ThisAction.instruction.description = "Add task step description here...";

            ActivityRecorderService activityService = ServiceManager.GetService<ActivityRecorderService>();

            ThisActionIndex = activityService.myActivity.actions.IndexOf(
                activityService.myActivity.actions.Where(a => a.id == ThisAction.id).FirstOrDefault());

            // update the singleton instance
            activityService.myActivity.actions[ThisActionIndex] = ThisAction;
        }


        private void UpdateWorkplaceInfo()
        {
            Vector3 myPos;
            Vector3 myRot;
            if (ServiceManager.GetService<ActivityRecorderService>().calibrationMarkerFound)
            {
                Transform originT = ServiceManager.GetService<ActivityRecorderService>().RecordingOrigin;

                // Modified by Jake to fix the calibration issue.
                // myPos = originT.InverseTransformPoint(this.gameObject.transform.position); // * originT.localScale.magnitude;
                // myRot = originT.InverseTransformDirection(this.transform.forward);

                // Just to check how the anchor is orientated in the HoloLens tracked space...
                Debug.Log("\n\n\n\n" + "\nANCHOR" + this.transform.name + "POSITION: " + this.transform.position + "\nANCHOR EULER ANGLES: " + this.transform.eulerAngles + "\nTARGET EULER ANGLES: " + originT.eulerAngles + "\n\n\n\n");         

                // Some black magic for getting the offset.
                var anchorDummy = new GameObject("AnchorDummy");
                var targetDummy = new GameObject("TargetDummy");

                anchorDummy.transform.position = this.transform.position;
                anchorDummy.transform.rotation = this.transform.rotation;
                targetDummy.transform.position = originT.transform.position;
                targetDummy.transform.eulerAngles = originT.transform.eulerAngles;

                anchorDummy.transform.SetParent(targetDummy.transform);

                myPos = anchorDummy.transform.localPosition;
                myRot = ConvertEulerAngles (anchorDummy.transform.localEulerAngles);

                Destroy(anchorDummy);
                Destroy(targetDummy);
                
                Debug.Log("task station position set as: " + myPos.ToString() + ", rotation is " + myRot.ToString());
            }
            else
            {
                myPos = this.gameObject.transform.position;
                myRot = this.gameObject.transform.rotation.eulerAngles;
            }
            

            

            thisPlace.id = TaskStationName;
            thisPlace.name = "";

            if (!hasMarker) // is either anchor id or marker id 
            {
                thisPlace.detectable = "WA-" + TaskStationId.ToString();

                thisDetectable.id = thisPlace.detectable;
                thisDetectable.sensor = "";
                thisDetectable.url = "";
                thisDetectable.type = "anchor";
                thisDetectable.origin_position = myPos.x + ", " + myPos.y + ", " + myPos.z;
                thisDetectable.origin_rotation = myRot.x + ", " + myRot.y + ", " + myRot.z;
            }
            else
            {
                thisPlace.detectable = "WA-" + TaskStationId.ToString();
                thisDetectable.id = thisPlace.detectable;
                thisDetectable.sensor = "";
                thisDetectable.url = "";            // set in SaveImagetarget()
                thisDetectable.type = "marker";
            }



            ActivityRecorderService activityService = ServiceManager.GetService<ActivityRecorderService>();

            thisPlaceIndex = activityService.myWorkplace.places.IndexOf(
                activityService.myWorkplace.places.Where(p => p.id == thisPlace.id).FirstOrDefault());
            if (thisPlaceIndex > -1)
            {
                activityService.myWorkplace.places[thisPlaceIndex] = thisPlace;
            }

            thisDetectIndex = activityService.myWorkplace.detectables.IndexOf(
                activityService.myWorkplace.detectables.Where(p => p.id == thisDetectable.id).FirstOrDefault());
            if (thisDetectIndex > -1)
            {
                activityService.myWorkplace.detectables[thisDetectIndex] = thisDetectable;
            }

        }

        public void SaveImagetarget(int markerId)
        {
            // string filename = "WEKITCalibrationTool_scaled.jpg";
            // thisDetectable.url = filename;
               
            // byte[] image = File.ReadAllBytes("resources://ImageTargets/" + filename);
            // string imagePath = Path.Combine(ServiceManager.GetService<ActivityService>().currentArlemFolder, filename); 
            // File.WriteAllBytes(imagePath, image);

        }


        #endregion ARLEM Data Handling


        #region Activate / Deactivate Functions

        public void ActivateTaskStation()
        {
            ServiceManager.GetService<ActivityRecorderService>().ActiveTaskStation = this.gameObject;

            for (int i = 0; i < this.transform.childCount; i++)
            {
                GameObject antn = this.transform.GetChild(i).gameObject;
                if (antn.name != "Number")
                {
                    antn.SetActive(true);
                }
            }

            if (highlighter == null)
            {
                highlighter = Instantiate(Resources.Load("Prefabs/SphereHighlight") as GameObject);
                highlighter.transform.position = this.gameObject.transform.position;
            }

        }

        public void DeactivateTaskStation()
        {
            // make inactive any annotations on the previously targeted TS (and close their menu, if it's open)
            // from 1, since the first child is animation number
            for (int i = 1; i < this.transform.childCount; i++)
            {
                GameObject antn = this.transform.GetChild(i).gameObject;
                
                if (antn.name != "Number")
                {
                    // if (antn.GetComponent<AnnotationController>().IsAnnotationMenuOpen)
                    // {
                    //     antn.GetComponent<AnnotationController>().ToggleAnnotationMenu();
                    // }
                    antn.SetActive(false);
                }
                
            }
            if (highlighter != null)
            {
                Destroy(highlighter);
                highlighter = null;
            }

            // update ARLEM info (saving changes)
            UpdateActionInfo();
            UpdateWorkplaceInfo();

            // set local flags
            currentAnnotation = null;
        }

        /// <summary>
        /// Delete the task station. The lines may not be updated.
        /// </summary>
        public void RemoveTaskStation()
        {
            // Update the lines
            Debug.Log(TaskStationName + "Removed");
            Destroy(transform.parent.gameObject);
        }
        #endregion Activate / Deactivate Functions

        
        #region Menu Toggle and Click Functions

        public void ToggleMenu()
        {

            if (IsMenuOpen)
            {
                // myTSCM.CloseMenu();
            }
            else
            {
                // myTSCM.OpenMenu();
            }

            IsMenuOpen = !IsMenuOpen;
        }

        /// <summary>
        /// Creates an annotation
        /// </summary>
        /// <param name="selectedMenuItem"></param>
        public void MenuItemClicked(GameObject selectedMenuItem)
        {
            /*
            // instantiate copy of selected menuitem and add to scene & list
            Vector3 startPos = selectedMenuItem.transform.position;
            GameObject newAnnotation = Instantiate(selectedMenuItem, startPos, Quaternion.identity);

            // wean new object
            newAnnotation.transform.Find("Ring").gameObject.SetActive(true);
            newAnnotation.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            newAnnotation.transform.parent = this.gameObject.transform;

            //face the user at creation
            Vector3 fwd = Camera.main.transform.forward;
            fwd.y = 0.0f;
            newAnnotation.transform.rotation = Quaternion.LookRotation(fwd);


            AnnotationBase[] annotations = newAnnotation.GetComponents<AnnotationBase>();

            for (int x = 0; x < annotations.Count(); x++)
            {
                AnnotationBase annotationBase = annotations[x];

                // create new arlem enter-exit items
                CreateArlemAnnotation();

                annotationBase.thisAnnotation = thisAction.enter.activates.Last() as ToggleObject;
                annotationBase.thisAnnotation.id = taskStationName;

                // give the annotation an arlem id
                newAnnotation.GetComponent<AnnotationController>().IdentifyAnnotation(ArlemAnnotationCounter, annotationBase);

                Debug.Log($"New {annotationBase.thisAnnotation.predicate} annotation registered, index: {ArlemAnnotationCounter}");
                ArlemAnnotationCounter++;
            }

            currentAnnotation = newAnnotation;
            annotationList.Add(currentAnnotation);

            Debug.Log($"{currentAnnotation.name} added to annotation view");

            // close menu
            ToggleMenu();
            */
        }

        #endregion Menu Toggle and Click Functions
        

        #region Interface Functions

        /// <summary>
        /// called, when the user gazes at the object. Applies FocusSizeFactor and FocusColor
        /// </summary>
        public void OnFocusEnter(FocusEventData eventData)
        {
            this.gameObject.transform.localScale *= FocusSizeFactor;
            ChangeColor(FocusColor);
        }


        /// <summary>
        /// called, when the user's gaze leaves the object. Reduces size by FocusSizeFactor and applies currentColor.
        /// </summary>
        public void OnFocusExit(FocusEventData eventData)
        {
            this.gameObject.transform.localScale /= FocusSizeFactor;
            ChangeColor(startColour);
        }

        /// <summary>
        /// convenience method to apply a color to a renderer and all children's renderers.
        /// </summary>
        /// <param name="color">the color to apply.</param>
        public void ChangeColor(Color color)
        {
            if (this.gameObject.GetComponent<Renderer>() != null)
            {
                this.gameObject.GetComponent<Renderer>().material.color = color;
            }
        }

        /*
        /// <summary>
        /// Function to make decisions based on interaction - currently just air-tapping
        /// </summary>
        /// <param name="clickedObject"></param>
        public void ReportInputClick(GameObject clickedObject) // info passed from ActivityService
        {
            if (IsMenuOpen && myTSCM != null) // can't click on annotations when the menu is open
            {
                if (myTSCM.menuItemNames.Contains(clickedObject.name)) // user tapped a Task Station menu item (each is named with Guid)
                {
                    MenuItemClicked(clickedObject);
                }
            }

            else if (annotationList.Contains(clickedObject)) // or it's an annotation
            {
                currentAnnotation = clickedObject;
                currentAnnotation.GetComponent<AnnotationController>().AnnotationClicked();

            }

            else if (currentAnnotation != null) // it is another thing - likely an annotation menu control (pass across: less raycast approach)
            {
                currentAnnotation.GetComponent<AnnotationController>().ReportClick(clickedObject);
            }
        }
        */
        #endregion Interface Functions

        #region Utilities

        private Vector3 ConvertEulerAngles(Vector3 angles)
        {
            var output = new Vector3
            {
                x = ConvertSingleAxis(angles.x),
                y = ConvertSingleAxis(angles.y),
                z = ConvertSingleAxis(angles.z)
            };

            return output;
        }

        private float ConvertSingleAxis(float angle)
        {
            if (angle < 0)
                angle += 360;
            else if (angle > 180)
                angle -= 360;

            float output = angle;
            return output;
        }

        #endregion Utilities
    }
}
