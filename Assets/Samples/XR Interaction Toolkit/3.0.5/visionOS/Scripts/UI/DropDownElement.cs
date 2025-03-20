using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Drop down element interactable. Used to represent an element in a dropdown list.
    /// </summary>
    public class DropDownElement : XRBaseInteractable
    {
        [Header("Dropdown Element Config")]
        [SerializeField]
        string m_DrownDownText;
        
        public string dropDownText => m_DrownDownText;

        [SerializeField]
        Renderer m_ElementRenderer;
        
        [Header("Toggle Colors")]
        [SerializeField]
        Color m_SelectedColor = new Color(.1254f, .5882f, .9529f);
        
        [SerializeField]
        Color m_UnselectedColor = new Color(.1764f, .1764f, .1764f);

        Material m_MaterialInstance;
        
        void Start()
        {
            m_MaterialInstance = m_ElementRenderer.material;
        }

        public void SetSelected(bool isElementSelected)
        {
            m_MaterialInstance.color = isElementSelected ? m_SelectedColor : m_UnselectedColor;
        }
    }
}