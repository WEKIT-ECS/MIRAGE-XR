using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Base class for all the MirageXR prefabs.
    /// </summary>
    public abstract class MirageXRPrefab : BaseFocusHandler
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private GameObject _center;
        private LineRenderer _lineRenderer;
        private Transform _centerOfView;
        private bool _isVisible;
        private bool _isFound;


        // for gazing trigger
        private GameObject gazeCircle;
        private List<GameObject> myColliderChilren;

        private GameObject _guideLine;

        // Should the gaze guiding be used or not.
        protected bool UseGuide = false;

        public LearningExperienceEngine.ToggleObject Annotation { get; private set; }

        // events that register focus even
        public delegate void FocusChangedEventHandler(string pointer, GameObject focusedObject, Vector3 hitPoint);
        public FocusChangedEventHandler AugmentationGotFocus;
        public FocusChangedEventHandler AugmentationLostFocus;


        private void OnEnable()
        {
            EventManager.OnShowGuides += ShowGuides;
            EventManager.OnHideGuides += HideGuides;
            LearningExperienceEngine.EventManager.OnClearPois += Delete;
            LearningExperienceEngine.EventManager.OnClearAll += Delete;
        }

        private void OnDisable()
        {
            EventManager.OnShowGuides -= ShowGuides;
            EventManager.OnHideGuides -= HideGuides;
            LearningExperienceEngine.EventManager.OnClearPois -= Delete;
            LearningExperienceEngine.EventManager.OnClearAll -= Delete;
        }

        public override void OnFocusEnter(FocusEventData eventData)
        {
            var pointerResult = eventData.Pointer.Result.Details;
            GameObject hitObject = pointerResult.Object;
            Vector3 hitPoint = pointerResult.Point;

            //Debug.Log("Pointer:" + eventData.Pointer.PointerName + "\nObject: " + hitObject.name + "\nHit Point: " + hitPoint.ToString());

            AugmentationGotFocus?.Invoke(eventData.Pointer.PointerName, hitObject, hitPoint);
        }

        public override void OnFocusExit(FocusEventData eventData)
        {
            AugmentationLostFocus?.Invoke(eventData.Pointer.PointerName, eventData.OldFocusedObject, eventData.Pointer.Result.Details.Point);
        }

        private void Awake()
        {
            if (UseGuide)
            {
                // Add the line renderer.
                gameObject.AddComponent<LineRenderer>();

                // Configure the line renderer.
                _lineRenderer = GetComponent<LineRenderer>();

                // Just to prevent unaccurate guiding before the system has any idea where the object is...
                _lineRenderer.enabled = false;

                _lineRenderer.material.color = Color.white;

                // Set line width
                _lineRenderer.startWidth = 0.0005f;
                _lineRenderer.endWidth = 0.0005f;

                // Get the center of the view.
                _centerOfView = GameObject.FindGameObjectWithTag("MainCamera").transform;

                // Add and configure the necessary helper.
                _center = Instantiate(Resources.Load<GameObject>("GazeGuide"), transform);
                _center.name = "GazeGuide";
                _center.transform.localPosition = Vector3.zero;
                _center.transform.localEulerAngles = Vector3.zero;
                _center.transform.localScale = Vector3.one;
                _center.GetComponent<GazeGuide>().Parent = gameObject;
                _center.SetActive(true);
            }
        }

        /// <summary>
        /// Show guideline.
        /// </summary>
        private void ShowGuides()
        {
            if (_guideLine != null)
                _guideLine.SetActive(true);
        }

        // Hide guideline.
        private void HideGuides()
        {
            if (_guideLine != null)
                _guideLine.SetActive(false);
        }

        public bool SetGuide(LearningExperienceEngine.ToggleObject obj)
        {
            // Find the target object.
            var targetObject = GameObject.Find(obj.id + "/default");

            // If target object not found, break out.
            if (targetObject == null)
                return false;

            // Instantiate guideline.
            _guideLine = Instantiate(Resources.Load<GameObject>("ThinLine"), Vector3.zero, Quaternion.identity);

            _guideLine.name = "ThinLine";

            // Set guideline as a child of current gameobject.
            _guideLine.transform.SetParent(gameObject.transform);

            // Set position.
            _guideLine.transform.localPosition = Vector3.zero;

            // Set rotation.
            _guideLine.transform.localEulerAngles = Vector3.zero;
            _guideLine.GetComponent<SetPoints>().targetTransform = targetObject.transform;

            // Hide by default.
            _guideLine.SetActive(false);

            // Reactivate only if find is active or if the activity is forcing the guide.
            if (GameObject.Find("UiManager").GetComponent<UiManager>().IsFindActive || obj.guide)
            {
                // If guide is forced by the activity, enable also the main direction line for the parent object.
                if (obj.guide)
                {
                    // GameObject.Find(obj.id).transform.GetComponentInChildren<PathRoleController>(true).IsVisible = true;

                    // UIOrigin.Instance.ShowOrigin();

                    // Set tag.
                    transform.tag = "GuideActive";
                }

                // Set inactive tag if guide not forced.
                else
                    transform.tag = "GuideInactive";


                // In all cases, enable the thinner direction line.
                _guideLine.SetActive(true);
            }

            // Everything is ok!
            return true;
        }

        /// <summary>
        /// Just for enabling the gaze guide when needed.
        /// </summary>
        public void EnableGazeGuide()
        {
            // A little hack to make this usable also with the detect type...
            var line = GetComponent<LineRenderer>();

            if (line != null)
                line.enabled = true;
        }

        private void Update()
        {
            if (_lineRenderer != null)
            {
                // If object has not been detected...
                if (_lineRenderer.enabled)
                {
                    // Draw a line between the object position and the center of the view.
                    _lineRenderer.SetPosition(0, transform.position);
                    _lineRenderer.SetPosition(1, _centerOfView.position);
                }
            }


            if (gameObject)
            {
                Vector3 screenPoint = Camera.main.WorldToViewportPoint(gameObject.transform.position);
                bool iAmOnScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                if (iAmOnScreen && myColliderChilren != null)
                {
                    if (IsGazeTrigger())
                    {
                        DetectLabelOnGaze();
                    }

                }
            }

        }

        /// <summary>
        /// All the prefabs has to implement an initialization method.
        /// </summary>
        public virtual bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            Annotation = obj;
            StartCoroutine(FindColliderChildren());
            return true;
        }

        /// <summary>
        /// Default delete functionality for all the MirageXR prefab objects.
        /// </summary>
        public virtual void Delete()
        {
            if (gameObject)
                Destroy(gameObject);
        }

        /// <summary>
        /// Set prefab parent.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns false if the parent can't be set.</returns>
        protected bool SetParent(LearningExperienceEngine.ToggleObject obj)
        {
            if (obj.id.Equals("UserViewport"))
            {
                var tempTransform = GameObject.FindGameObjectWithTag("UserViewport").transform;
                var tempPosition = tempTransform.position;
                var tempRotation = tempTransform.eulerAngles;
                tempRotation.z = 0;

                if (!string.IsNullOrEmpty(obj.position))
                {
                    tempPosition += Utilities.ParseStringToVector3(obj.position);
                }

                if (!string.IsNullOrEmpty(obj.rotation))
                {
                    tempRotation += Utilities.ParseStringToVector3(obj.rotation);
                }

                transform.position = tempPosition;
                transform.eulerAngles = tempRotation;

                if (gameObject.GetComponent<Billboard>() == null)
                {
                    gameObject.AddComponent<Billboard>();
                }
            }
            else
            {
                // If poi not set, assume that default poi is meant to be used.
                if (string.IsNullOrEmpty(obj.poi))
                {
                    obj.poi = "default";
                }

                var temp = GameObject.Find($"{obj.id}/{obj.poi}");
                if (temp == null)
                {
                    Debug.Log("Parent object not found. " + obj.id + "/" + obj.poi);
                    return false;
                }

                transform.SetParent(temp.transform);

                transform.localPosition = Vector3.zero;

                if (obj.scale == 0)
                {
                    transform.localScale = Vector3.one;
                }

                if (!string.IsNullOrEmpty(obj.position))
                {
                    transform.localPosition = Utilities.ParseStringToVector3(obj.position);
                }

                if (!string.IsNullOrEmpty(obj.rotation) && Utilities.TryParseStringToQuaternion(obj.rotation, out var myRotation))
                {
                    transform.localRotation = myRotation;
                }
                else
                {
                    transform.localEulerAngles = Vector3.zero;
                }
            }

            // If everything was ok, return true.
            return true;
        }


        /// <summary>
        /// Reads and convert the PoiEditor's scale factor,
        /// checking for null values and, in case of error,
        /// reverting to the given default value.
        /// </summary>
        /// <param name="poiEditor"></param>
        /// <param name="defaultScale"></param>
        /// <returns>Returns the desired Vector3 scale.</returns>
        protected static Vector3 GetPoiScale(PoiEditor poiEditor, Vector3 defaultScale)
        {
            // since scaling is activated, allow the poi editor's object manipulator to set it.
            poiEditor.canScale = true;
            LearningExperienceEngine.Poi poi = poiEditor.GetMyPoi();

            // ensure relevant string has value
            if (string.IsNullOrEmpty(poi.scale))
            {
                return defaultScale;
            }

            var poiScale = Utilities.ParseStringToVector3(poi.scale);

            // check for zero-scale values
            if (poiScale.x == 0f || poiScale.y == 0f || poiScale.z == 0f)
            {
                return defaultScale;
            }

            return poiScale;
        }

        /// <summary>
        /// Reads and convert the PoiEditor's rotation factor,
        /// checking for null values. If not set, reverts to
        /// euler angle version of Quaternion.identity.
        /// </summary>
        /// <param name="poiEditor"></param>
        /// <returns>Returns the desired rotation in euler angles.</returns>
        protected static Vector3 GetPoiRotation(PoiEditor poiEditor)
        {
            // since scaling is activated, allow the poi editor's object manipulator to set it.
            poiEditor.canRotate = true;
            LearningExperienceEngine.Poi poi = poiEditor.GetMyPoi();

            // ensure relevant string has value
            return string.IsNullOrEmpty(poi.rotation) ? Vector3.zero : Utilities.ParseStringToVector3(poi.rotation);
        }

        private bool IsGazeTrigger()
        {
            return activityManager.ActiveAction.triggers.Find(t => t.id == Annotation.poi) != null
                   && Annotation.predicate != "video" && Annotation.predicate != "audio" && !Annotation.predicate.StartsWith("char") && Annotation.predicate != "pickandplace";
        }


        private IEnumerator FindColliderChildren()
        {
            myColliderChilren = new List<GameObject>();

            foreach (var child in GetComponentsInChildren<Collider>())
            {
                var meshCollider = child.GetComponent<MeshCollider>();
                if (meshCollider)
                {
                    meshCollider.convex = true;
                }
                myColliderChilren.Add(child.gameObject);
            }
            yield return null;
        }

        private void DetectLabelOnGaze()
        {
            RaycastHit hit;
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out hit, 10.0f);
            }
            else
            {
                Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, 10.0f);
            }

            if (hit.collider && myColliderChilren.Contains(hit.collider.gameObject))
            {
                var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == Annotation.poi);
                int.TryParse(trigger.value, out int triggerStepNumber);

                if (triggerStepNumber > 0)
                    triggerStepNumber -= 1; // -1 for converting to the correct index

                if (!gazeCircle && !activityManager.EditModeActive && activityManager.ActionsOfTypeAction.Count > triggerStepNumber)
                {
                    gazeCircle = Instantiate(Resources.Load<GameObject>("Prefabs/UI/GazeSpinner"), hit.collider.transform.position, transform.rotation);
                    gazeCircle.AddComponent<Billboard>();
                    gazeCircle.GetComponent<GazeSpinner>().Duration = trigger.duration;
                    gazeCircle.GetComponent<GazeSpinner>().stepNumber = triggerStepNumber;
                    gazeCircle.transform.SetParent(transform);
                }
            }
            else
            {
                if (gazeCircle)
                {
                    Destroy(gazeCircle);
                }
            }
        }
    }
}