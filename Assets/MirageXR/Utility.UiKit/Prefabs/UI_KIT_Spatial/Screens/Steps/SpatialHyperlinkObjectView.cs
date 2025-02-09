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

        public void Start()
        {
            _camera = RootObject.Instance.BaseCamera;
            InitializeManipulator();
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
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            var generalGrabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            generalGrabTransformer.allowTwoHandedScaling = true;

            var xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            xrGrabInteractable.trackRotation = false;
            xrGrabInteractable.trackScale = false;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());
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
