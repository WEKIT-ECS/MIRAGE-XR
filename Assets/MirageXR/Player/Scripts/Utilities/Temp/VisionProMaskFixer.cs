using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MirageXR
{
    public class VisionProMaskFixer : MonoBehaviour
    {
        [SerializeField] private MaskableGraphic maskableGraphic;
        [SerializeField] private UIBehaviour uiMask;

        private void Awake()
        {
#if VISION_OS
            Destroy(maskableGraphic);
            Destroy(uiMask);
            gameObject.AddComponent<RectMask2D>();
#endif
            Destroy(this);
        }
    }
}
