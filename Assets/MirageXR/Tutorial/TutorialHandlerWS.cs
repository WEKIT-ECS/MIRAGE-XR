using MirageXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TutorialHandlerWS
{
    private TutorialArrow _currentArrow;
    private TutorialArrowFactory.ArrowType _currentArrowType;

    public bool PointTo(
        GameObject target,
        string message,
        TutorialArrowFactory.ArrowType arrowType = TutorialArrowFactory.ArrowType.DEFAULT,
        Vector3? arrowPositionOffset = null,
        Vector3? arrowRotationOffset = null
        )
    {
        if (target == null)
        {
            Debug.LogError("TutorialHandlerWS cannot PointTo null object.");
            return false;
        }

        if (_currentArrow == null || arrowType != _currentArrowType)
        {
            if (_currentArrow != null)
            {
                Object.Destroy(_currentArrow.gameObject);
            }
            var factory = TutorialArrowFactory.Instance();
            _currentArrow = factory.CreateArrow(arrowType);
        }

        _currentArrow.PointTo(target, message, arrowPositionOffset, arrowRotationOffset);

        return true;
    }

    public void Hide()
    {
        if (_currentArrow != null)
        {
            _currentArrow.Dissapear();
        }
    }
}
