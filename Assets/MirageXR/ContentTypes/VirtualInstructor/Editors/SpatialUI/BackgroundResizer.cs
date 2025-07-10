using UnityEngine;

namespace MirageXR
{
    public class BackgroundResizer : MonoBehaviour
    {

        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform shadow;
        [SerializeField] private RectTransform content;
        
        void Update() //TODO: remove this 
        {
            float contentHeight = content.sizeDelta.y; 
            shadow.sizeDelta = new Vector2(shadow.sizeDelta.x, contentHeight);
            background.sizeDelta = new Vector2(background.sizeDelta.x, contentHeight);
        }
    }
}
