using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ActivityRecorderService : IService
    {

        public enum TrialPartner
        {
            Altec = 0,
            Ebit = 1,
            LuftTransport = 2
        }

        public TrialPartner CurrentTrialPartner;

        // private VestSensor myVest;
        private GameObject debugText { get; set; }
        private Text txtDebug;

        [SerializeField] private GameObject worldOriginMarkerPrefab;
        [SerializeField] private GameObject taskStationPrefab;

        private RaycastHit hitInfo;
        private TapRecognizerService tapRecognizer;

        public static System.Collections.ObjectModel.ObservableCollection<GameObject> taskStationList =
        new System.Collections.ObjectModel.ObservableCollection<GameObject>();

        public GameObject ActiveTaskStation { get; set; }

        private int idx;
        private LineRenderer lineRenderer;
        private Vector3[] Points;

        private ToggleObject exitActivateElement; // used to link action id to previous action-exit-activate loop

        /// <summary>
        /// ARLEM Activity Data Object (top level)
        /// </summary>
        public Activity myActivity;
        public Workplace myWorkplace;

        public string dirBase;
        public string CurrentArlemFolder { set; get; }

        public Transform RecordingOrigin;
        public Vector3 RecordingEuler = Vector3.zero;
        public bool calibrationMarkerFound = false;
        public bool RecordingUnderWay = false;
        private Vector3 calibrationMarkerLocation;

        private GameObject worldOrigin;

        public MixedRealityInputAction inputAction;

        public void Initialize(IServiceManager owner)
        {
            worldOrigin = ObjectPool<GameObject>.RequestResource(() => { return new GameObject(); });
            worldOrigin.name = "World Origin";

            // myVest = new VestSensor();
            // myVest.StartCapture();

            debugText = GameObject.Find("HUD/Text");
            txtDebug = debugText.GetComponent<Text>();

            taskStationList.CollectionChanged += TaskStations_CollectionChanged;

            GameObject.DontDestroyOnLoad(worldOrigin);

            ServiceManager.GetService<WorldAnchorService>().Manager.AttachAnchor(worldOrigin, "worldOrigin");

            if (AppSelection.ActivityToLoad != null)
            {
                // myActivity = LoadModelFromJSON(AppSelection.activityToLoad);

            } // if loading existing activity

            else
            {
                myActivity = new Activity();
                myWorkplace = new Workplace();
                RecordingUnderWay = true;

                DateTime nowDate = DateTime.Now;
                dirBase = "session-" + nowDate.ToString("yyyy-MM-dd_HH-mm-ss");

                CurrentArlemFolder = Application.persistentDataPath + "/" + dirBase;
                Directory.CreateDirectory(CurrentArlemFolder);

            } // if new activity

            tapRecognizer = ServiceManager.GetService<TapRecognizerService>();
            tapRecognizer.DoubleTapRecognized += DoubleTapRecognized;
            
#if UNITY_ANDROID || UNITY_IOS
            tapRecognizer.TapRecognized += TapRecognized;
#endif
            Maggie.Speak("Look at the calibration marker to begin.");
        }

        public void Cleanup()
        {
            tapRecognizer.DoubleTapRecognized -= DoubleTapRecognized;

            ClearScene();

            if (worldOrigin != null)
            {
                worldOrigin.name = "GameObject";
                ObjectPool<GameObject>.ReleaseResource(worldOrigin);
            }
        }

        public void UpdateSessionName(string newName)
        {
            myActivity.name = newName;
        }

        public void SetRecordingOrigin(int trackableID, Vector3 location, Quaternion rotation)
        {
            if (!calibrationMarkerFound && RecordingUnderWay)
            {
                Debug.Log($"ACTIVITYSERVICE: trackable set at location: {location}");

                RecordingOrigin = new GameObject().transform;
                RecordingOrigin.position = location;

                RecordingOrigin.rotation = rotation;

                // Modified by Jake to get the calibration issue fixed.
                // RecordingOrigin.Rotate(90f, 0f, 0f); // We need to have the rotation as it is detected in Unity!

                GameObject.Instantiate(worldOriginMarkerPrefab, location, rotation); // can be removed - only for reference.

                calibrationMarkerFound = true;
                Debug.Log("the origin's scale factor is " + RecordingOrigin.localScale.ToString());

                Maggie.Speak("Calibration complete. Double tap to place a task station.");
                // taskStationList[0].GetComponent<TaskStationController>().SaveImagetarget(trackableID);
            }
        }

        private void TaskStations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.Log($"ACTIVITYSERVICE: taskstation list changed, now has {taskStationList.Count} entries");

            for (int i = 0; i < taskStationList.Count; i++)
            {
                taskStationList[i].GetComponentInChildren<TextMesh>().text = i.ToString();
            }

            if (taskStationList.Count >= 2)
            {
                Points = new Vector3[taskStationList.Count];
                DrawLines();
            }
        }


        private void DrawLines()
        {
            // only connects the last task station to allow editing
            if (taskStationList[0].GetComponent<LineRenderer>() == null)
            {
                lineRenderer = taskStationList[0].AddComponent<LineRenderer>();
            }
            else
            {
                lineRenderer = taskStationList[0].GetComponent<LineRenderer>();
            }

            lineRenderer.material = new Material(Shader.Find("Standard"));
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.startColor = new Color(255f, 255f, 255f, 128f);
            lineRenderer.endColor = new Color(255f, 255f, 255f, 128f);

            for (int i = taskStationList.Count - 1; i >= 0; i--)
            {
                // since each positioncount in linerenderer only needs one point to draw one segment we do not store the point of the last
                Points[i] = taskStationList[i].GetComponent<Renderer>().bounds.center;
            }

            lineRenderer.positionCount = taskStationList.Count - 1;
            lineRenderer.SetPositions(Points);
        }


        /// <summary>
        /// adds a new annotatable game object to the scene and positions it in the field of vision of the user.
        /// </summary>
        private void AddTaskStation(Vector3 hitPoint, bool hasImageMarker = false)
        {
            // Create task stations only if the keyboard is not active to prevent accidental task stations on double letters...
            NonNativeKeyboard keyboard = NonNativeKeyboard.Instance;

            if (keyboard == null || !keyboard.isActiveAndEnabled)
            {
                if (taskStationList.Count == 0 && !calibrationMarkerFound)
                {
                    Maggie.Speak(
                        "Your workplace is not calibrated.  Recordings cannot be played back on other devices or in other workplaces.");
                }

                // close any menus that are open
                foreach (GameObject ts in taskStationList.Where(t =>
                    t.GetComponent<TaskStationController>().IsMenuOpen == true))
                {
                    ts.GetComponent<TaskStationController>().ToggleMenu();
                }

                // update the exit-activate ARLEM section of previous TS to point to the new one
                if (taskStationList.Count > 0 && ActiveTaskStation != null)
                {
                    // update the exit-activate ARLEM section of previous TS to point to the new one
                    if (idx >= 0 && idx < taskStationList.Count)
                    {
                        myActivity.actions[taskStationList.IndexOf(ActiveTaskStation)].exit.activates
                            .Add(new Activate());
                        exitActivateElement = myActivity.actions[taskStationList.IndexOf(ActiveTaskStation)].exit
                            .activates.Last();
                    }
                }

                // update exit-activate section and deactivate the current task station
                if (ActiveTaskStation != null)
                {
                    ActiveTaskStation.GetComponent<TaskStationController>().DeactivateTaskStation();
                }

                // create a new workplace place
                myWorkplace.places.Add(new Place());
                myWorkplace.detectables.Add(new Detectable());

                // create a new arlem action representing the task station
                // (must be done before instantiating the object)
                myActivity.actions.Add(new Action());
                myActivity.actions.Last().triggers = new List<Trigger>();

                // instantiate task station
                GameObject newTS = GameObject.Instantiate(taskStationPrefab, worldOrigin.transform, true);

                // set task station as active child of the gameobject (called 'ActivityModel' in Unity)
                newTS.SetActive(true);
                newTS.GetComponent<TaskStationController>().hasMarker = hasImageMarker;

                // place the task station
                float minDistanceToObstacle = 0.2f;
                Vector3 targetPosition = hitPoint - (Camera.main.transform.forward * minDistanceToObstacle);
                newTS.transform.position = targetPosition;

                // now link the new TSs name to the previous exit-activate section in ARLEM
                if (taskStationList.Count > 0)
                {
                    Action newTSAction = newTS.GetComponent<TaskStationController>().ThisAction;
                    exitActivateElement.id = newTSAction.id;
                    exitActivateElement.viewport = newTSAction.viewport;
                    exitActivateElement.type = newTSAction.type;

                    myActivity.actions[(taskStationList.IndexOf(ActiveTaskStation))].exit.activates[0] =
                        exitActivateElement;
                }

                // updates local variables for object tracking
                taskStationList.Add(newTS);
                ActiveTaskStation = newTS;

                idx = taskStationList.IndexOf(ActiveTaskStation);
                Debug.Log("Added " + newTS.name + " to list of task stations");
            }
        }

        public void RemoveActiveTaskStation()
        {
            int TS2delete = taskStationList.IndexOf(ActiveTaskStation);
            int totalTSs = taskStationList.Count;

            GameObject.Destroy(ActiveTaskStation.GetComponent<TaskStationController>().highlighter);
            GameObject.Destroy(ActiveTaskStation);

            Debug.Log($"Planning to delete TS with index {TS2delete}");

            if (TS2delete == totalTSs - 1)          // if deleting the last TS
            {
                Debug.Log("deleting last ts");
                myActivity.actions[TS2delete].exit.activates.Last().id = "";
                myActivity.actions.Remove(myActivity.actions[TS2delete]);

                if (totalTSs != 1)
                {
                    Debug.Log("switching active taskstation (back)");
                    taskStationList[TS2delete - 1].GetComponent<TaskStationController>().ActivateTaskStation();
                }
            }

            else if (TS2delete == 0)                // if deleting the first TS
            {
                Debug.Log("deleting first ts");
                // update start TS with the one that is currently second (prior to deleting the TS, in this case)
                myActivity.start = myActivity.actions[1].id;
                myActivity.actions.Remove(myActivity.actions[TS2delete]);

                if (totalTSs != 1)
                {
                    Debug.Log("switching active taskstation (forward)");
                    taskStationList[TS2delete + 1].GetComponent<TaskStationController>().ActivateTaskStation();
                }
            }

            else                                    // we are deleting one that is in the middle
            {
                myActivity.actions[TS2delete - 1].exit.activates.Last().id = myActivity.actions[TS2delete + 1].id;
                myActivity.actions.Remove(myActivity.actions[TS2delete]);
                taskStationList[TS2delete - 1].GetComponent<TaskStationController>().ActivateTaskStation();
            }

            // remove arlem action
            // myActivity.actions.Remove(myActivity.actions[TS2delete]);

            taskStationList.Remove(taskStationList[TS2delete]);

        }


        public void UploadARLEMData()
        {
            // close any menus that are open
            foreach (GameObject ts in taskStationList.Where(t => t.GetComponent<TaskStationController>().IsMenuOpen == true))
            {
                ts.GetComponent<TaskStationController>().ToggleMenu();

            }
            // myVest.StopCapture();
            UpdateDataModel();

            int idx = CurrentArlemFolder.LastIndexOf('/') + 1;
            string arlemId = CurrentArlemFolder.Substring(idx);
            Debug.Log("Zipping and Uploading " + CurrentArlemFolder + " to " + arlemId);
            Network.Upload(CurrentArlemFolder, arlemId, httpStatusCode => {
                if (httpStatusCode == HttpStatusCode.OK)
                {
                    Maggie.Speak("Save complete.");
                    GameObject.Find("RecorderControlPanel/Buttons/SaveArlem/ObjectText").GetComponent<Text>().text = "Save Recording";
                }
                else
                {
                    Maggie.Speak($"Uploading ARLEM failed. Check your system administrator. {httpStatusCode}");
                    GameObject.Find("RecorderControlPanel/Buttons/SaveArlem/ObjectText").GetComponent<Text>().text = "Error, try again";
                }
            });
            Debug.Log("Upload ARLEM done.");
        }


        public void UpdateDataModel()
        {
            if (ActiveTaskStation != null)
            {
                ActiveTaskStation.GetComponent<TaskStationController>().DeactivateTaskStation();
            }

            txtDebug.text = "";

            // add activity level ARLEM details
            myActivity.id = dirBase;
            // myActivity.name = "WEKIT Trial - " + dirBase;
            myActivity.language = "English";
            myActivity.start = taskStationList.First().GetComponent<TaskStationController>().ThisAction.id;

            // add workplace (top level) details
            myWorkplace.id = $"{dirBase}-workplace.json";
            myWorkplace.name = "unnamed";

            // link workplace to activity
            myActivity.workplace = myWorkplace.id;
            PlayerPrefs.SetInt(myWorkplace.id, 1);

            if (taskStationList.Count == 0)
            {
                return;
            }

            // displaying the activity outline on the text debug HUD object
            foreach (GameObject taskStation in taskStationList)
            {
                // update the data model - belt and braces, like
                TaskStationController tsc = taskStation.GetComponent<TaskStationController>();
                int tsIndex = tsc.ThisActionIndex;
                myActivity.actions[tsIndex] = tsc.ThisAction;
            }

            // Update ARLEM shtuff
            SaveActivityData();
            SaveWorkplaceData();
        }

        private void SaveActivityData()
        {
            var recFilePath = Path.Combine(Application.persistentDataPath, $"{dirBase}-activity.json"); // myActivity.id
            var json = JsonConvert.SerializeObject(myActivity);
            File.WriteAllText(recFilePath, json);
        }

        private void SaveWorkplaceData()
        {
            var wplFilePath = Path.Combine(Application.persistentDataPath, $"{dirBase}-workplace.json"); // myWorkplace.id
            var json = JsonConvert.SerializeObject(myWorkplace);
            File.WriteAllText(wplFilePath, json);
        }


        private void TaskStationClicked(GameObject selectedObject)
        {
            // in case the active task station has been deleted.
            if (ActiveTaskStation == null)
            {
                ActiveTaskStation = taskStationList.Last();
            }

            // if the gaze-selected object is not the current taskstation, deactivate the current one and set this one as the active one (but don't open the menu)
            if (selectedObject != ActiveTaskStation)
            {
                TaskStationController prevTS = ActiveTaskStation.GetComponent<TaskStationController>();
                if (prevTS.IsMenuOpen) { prevTS.ToggleMenu(); }
                prevTS.DeactivateTaskStation();

                TaskStationController currentTS = selectedObject.GetComponent<TaskStationController>(); ;
                currentTS.ActivateTaskStation();
                ActiveTaskStation = selectedObject;
            }
            else // open the menu
            {
                TaskStationController myController = ActiveTaskStation.GetComponent<TaskStationController>();
                myController.ActivateTaskStation();
                myController.ToggleMenu();
            }
        }


        private void DoubleTapRecognized(object sender, TapEventArgs e)
        {
            Debug.Log("Double tap recognized");
            Debug.Log("Object hit: " + e.SelectedObject);

            if (e.SelectedObject == null)
            {
                Maggie.Speak("Scan more of your environment to place objects on real surfaces.");
                return;
            }

            if (taskStationList.Contains(e.SelectedObject))
            {
                TaskStationClicked(e.SelectedObject);
            }
            else if (e.SelectedObject.layer == LayerMask.NameToLayer("Spatial Awareness"))
            {
                AddTaskStation(e.HitPoint);
            }

            if (ActiveTaskStation != null) // pass to task station controller for checking 
            {
                // activeTaskStation.GetComponent<TaskStationController>().ReportInputClick(e.SelectedObject);
            }
        }
        
        private void TapRecognized(object sender, TapEventArgs e)
        {
            Debug.Log("tap recognized");
            Debug.Log("Object hit: " + e.SelectedObject);

            if (e.SelectedObject == null)
            {
                return;
            }

            if (taskStationList.Contains(e.SelectedObject))
            {
                TaskStationClicked(e.SelectedObject);
            }

            if (ActiveTaskStation != null) // pass to task station controller for checking 
            {
                // activeTaskStation.GetComponent<TaskStationController>().ReportInputClick(e.SelectedObject);
            }
        }

        // public void ActOnTaps(InteractionSourceKind source, int tapCount, Ray headRay)
        // {
        //     Debug.Log("Tap recognized");
        //     if (Physics.Raycast(headRay, out hitInfo, 10.0f, Physics.DefaultRaycastLayers)) // the raycast hit an augmentation
        //     {
        //         focusedObject = hitInfo.collider.gameObject;
        //         Debug.Log("Object hit: " + focusedObject.name);
           
        //         if (focusedObject.transform.parent != null)
        //         {
        //             if (focusedObject.transform.parent.name == "SpatialMapping" && tapCount > 1) // the user double-tapped the spatial map
        //             {
        //                 AddTaskStation(hitInfo.point);
        //             }
           
        //             else if (taskStationList.Contains(focusedObject)) // the user tapped a task station => open the menu
        //             {
        //                 TaskStationClicked(focusedObject);
        //             }
        //         }
           
        //         if (activeTaskStation != null) // pass to task station controller for checking 
        //         {
        //             activeTaskStation.GetComponent<TaskStationController>().ReportInputClick(focusedObject, tapCount, hitInfo);
        //         }
        //     }
        // }


        public void ClearScene()
        {
            // close any menus that are open
           foreach (GameObject ts in taskStationList.Where(t => t.GetComponent<TaskStationController>().IsMenuOpen == true))
            {
                ts.GetComponent<TaskStationController>().ToggleMenu();
            }

            if (worldOrigin != null)
            {
                // destroy all existing task stations
                for (int x = worldOrigin.transform.childCount - 1; x >= 0; x--)
                {
                    GameObject ts = worldOrigin.transform.GetChild(x).gameObject;
                    GameObject.Destroy(ts.GetComponent<TaskStationController>().highlighter);
                    GameObject.Destroy(ts);
                }
            }
            ActiveTaskStation = null;
            taskStationList.Clear();

            myActivity.actions.Clear();
            myWorkplace.places.Clear();
            myWorkplace.detectables.Clear();
        }
    }   // activityservice class

}   // namespace
