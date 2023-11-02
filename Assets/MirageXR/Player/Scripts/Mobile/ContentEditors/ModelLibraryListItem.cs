using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class ModelLibraryListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image thumbnail;
        [SerializeField] private Button itemButton;

        public TextMeshProUGUI Title => title;
        public Image Thumbnail => thumbnail;

        public void AddButtonListener(UnityAction action)
        {
            if (itemButton)
            {
                itemButton.onClick.AddListener(action);
            }
        }
    }

}
