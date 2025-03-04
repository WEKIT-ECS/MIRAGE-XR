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
        }
        
        private void OnEditorModeChanged(bool value)
        {
            _isCanvasTransformLocked = value;
            UpdateCanvasLockState();
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

        private void OnManipulationEnded() { }

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
