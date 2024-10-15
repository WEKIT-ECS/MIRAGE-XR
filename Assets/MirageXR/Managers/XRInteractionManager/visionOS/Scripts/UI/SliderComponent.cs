using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Slider component that can be poked or indirectly interacted with to change its value.
    /// Designed to work well with direct or indirect touch interaction.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class SliderComponent : XRBaseInteractable
    {
        [Header("Slider Configuration")]
        [SerializeField]
        MeshRenderer m_FillRenderer;

        [SerializeField]
        float m_SliderIndirectChangeScale = 1f;

        [SerializeField]
        float m_InitialSliderPercent = 0.5f;

        [SerializeField]
        FloatUnityEvent m_OnSliderValueChanged;

        float m_BoxColliderSizeX;
        int m_PercentageId;
        Material m_MaterialInstance;
        Vector3 m_StartLocalGrabPos;
        float m_Percentage;
        float m_OnGrabStartPercentage;

        void Start()
        {
            m_MaterialInstance = m_FillRenderer.material;
            m_PercentageId = Shader.PropertyToID("_Percentage");
            m_BoxColliderSizeX = GetComponent<BoxCollider>().size.x;
            SetFillPercentage(m_InitialSliderPercent);
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            m_OnGrabStartPercentage = m_Percentage;
            m_StartLocalGrabPos = transform.InverseTransformPoint(args.interactorObject.transform.position);
        }

        void Update()
        {
            if (!isSelected)
                return;

            var interactorSelecting = interactorsSelecting[0];
            var interactorPosition = interactorsSelecting[0].transform.position;

            if (interactorSelecting is IPokeStateDataProvider)
            {
                UpdateSliderAmtDirect(interactorPosition);
            }
            else
            {
                UpdateSliderAmtDelta(interactorPosition);
            }
        }

        void UpdateSliderAmtDirect(Vector3 interactorPos)
        {
            var localPosition = transform.InverseTransformPoint(interactorPos);
            var percentage = localPosition.x / m_BoxColliderSizeX + 0.5f;
            SetFillPercentage(percentage);
        }
        
        void UpdateSliderAmtDelta(Vector3 currentInteractorPos)
        {
            var currentLocalPos = transform.InverseTransformPoint(currentInteractorPos);
            var deltaVector = Vector3.ProjectOnPlane(currentLocalPos - m_StartLocalGrabPos, Vector3.up);
            var dx = deltaVector.x / m_BoxColliderSizeX;
            float newPercent = m_OnGrabStartPercentage + dx * m_SliderIndirectChangeScale;
            SetFillPercentage(newPercent);
        }

        float MapRange(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + ((value - inputMin) / (inputMax - inputMin) * (outputMax - outputMin));
        }
        
        void SetFillPercentage(float percentage)
        {
            m_Percentage = Mathf.Clamp01(percentage);
            m_MaterialInstance.SetFloat(m_PercentageId, MapRange(percentage, 0f, 1f, -0.01f, 1.01f));
            m_OnSliderValueChanged?.Invoke(1f - m_Percentage);
        }
    }
}