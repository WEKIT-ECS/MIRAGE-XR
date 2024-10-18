using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SelectAugmentationScreenSpatialView : MonoBehaviour
    {
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Transform _listContent;
        [SerializeField] private ContentSelectorListItem _contentSelectorListItemPrefab;
    }
}
