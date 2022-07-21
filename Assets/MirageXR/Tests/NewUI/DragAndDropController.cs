using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform currentTransform;
    
    private GameObject _mainContent;
    private Vector3 _currentPossition;
    private int _totalChild;
    private VerticalLayoutGroup _verticalLayoutGroup;

    public void OnPointerDown(PointerEventData eventData)
    {
        _currentPossition = currentTransform.position;
        _mainContent = currentTransform.parent.gameObject;
        _totalChild = _mainContent.transform.childCount;
        // the fix for overlapping items
        _verticalLayoutGroup = _mainContent.GetComponent<VerticalLayoutGroup>();
        _verticalLayoutGroup.childControlHeight = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentTransform.position =
            new Vector3(currentTransform.position.x, eventData.position.y, currentTransform.position.z);

        for (int i = 0; i < _totalChild; i++)
        {
            if (i != currentTransform.GetSiblingIndex())
            {
                Transform otherTransform = _mainContent.transform.GetChild(i);
                int distance = (int) Vector3.Distance(currentTransform.position,
                    otherTransform.position);
                if (distance <= 15)
                {
                    Vector3 otherTransformOldPosition = otherTransform.position;
                    otherTransform.position = new Vector3(otherTransform.position.x, _currentPossition.y,
                        otherTransform.position.z);
                    currentTransform.position = new Vector3(currentTransform.position.x, otherTransformOldPosition.y,
                        currentTransform.position.z);
                    currentTransform.SetSiblingIndex(otherTransform.GetSiblingIndex());
                    _currentPossition = currentTransform.position;
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentTransform.position = _currentPossition;
        _verticalLayoutGroup.childControlHeight = true;
    }
}
