using System;
using System.Text.RegularExpressions;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR
{
    public class SpatialHyperlinkObjectView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Transform diamond;
        
        private ActivityStep _step;
        private Camera _camera;
        private XRGrabInteractable _xrGrabInteractable;
        
        private bool _isCanvasTransformLocked = true;

        public void Start()
        {
            InitializeManipulator();
            _camera = RootObject.Instance.BaseCamera;
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
        }
        
        private void OnEditorModeChanged(bool value)
        {
            _isCanvasTransformLocked = value;
            UpdateCanvasLockState();
        }
        private void StepManagerOnStepChanged(ActivityStep step)
        {
            _step = step;
        }
        private void UpdateCanvasLockState()
        {
            _xrGrabInteractable.colliders.Clear();
            _xrGrabInteractable.enabled = _isCanvasTransformLocked;
        }
        
        public void SetText(string linkText)
        {
            if (text != null)
            {
                text.text = linkText;
            }
            else
            {
                Debug.LogError("TMP_Text component is not assigned in SpatialHyperlinkObjectView.");
            }
        }
        
        public void SetDiamondColor(Color diamondColor)
        {
            if (diamond != null)
            {
                var meshRenderer = diamond.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    var newMaterial = new Material(meshRenderer.sharedMaterial);
                    newMaterial.SetColor("_BaseColor", diamondColor); 
                    meshRenderer.material = newMaterial;
                }
                else
                {
                    Debug.LogError("MeshRenderer component not found on the diamond object.");
                }
            }
            else
            {
                Debug.LogError("Diamond object is not assigned in SpatialHyperlinkObjectView.");
            }
        }

        private void InitializeManipulator()
        {
            var rigidBody = this.gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            var generalGrabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            generalGrabTransformer.allowTwoHandedScaling = true;

            _xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            _xrGrabInteractable.trackRotation = false;
            _xrGrabInteractable.trackScale = false;
            _xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            _xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());

            _xrGrabInteractable.enabled = _isCanvasTransformLocked;
        }

        private void OnManipulationStarted() { }

       private void OnManipulationEnded()
       {
            if (transform == null)
            {
                return;
            }
            var currentPosition = transform.position;
            var currentLink = gameObject.name.Replace("hyperlink__", "");
            
            if (_step == null || string.IsNullOrEmpty(_step.Description))
            {
                return;
            }

            var savedText = _step.Description;
            string newText;
            var pattern = $@"(<color=[^>]+>\[{Regex.Escape(currentLink)}\]</color><pos=)(-?\d+\.?\d*),(-?\d+\.?\d*),(-?\d+\.?\d*)>";
            var match = Regex.Match(savedText, pattern);
            
            if (match.Success && match.Groups.Count >= 4)
            {
                try
                {
                    var prefix = match.Groups[1].Value;
                    var newPosTag = $"{prefix}{currentPosition.x:F2},{currentPosition.y:F2},{currentPosition.z:F2}>";
            
                    newText = savedText.Substring(0, match.Index) + newPosTag + savedText.Substring(match.Index + match.Length);
                    RootObject.Instance.LEE.StepManager.SetStepDescription(_step.Id, newText);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Unexpected error updating text: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"No matching pattern found for link {currentLink} in text.");
            }
       }

       private void LateUpdate()
       {
           DoTextBillboarding();
       }

       private void DoTextBillboarding()
       {
           var newRotation = _camera.transform.eulerAngles;
           newRotation.x = 0;
           newRotation.z = 0;
           text.transform.eulerAngles = newRotation;
       }
    }
}
