using MirageXR;
using UnityEngine;

/// <summary>
/// Handler for World-Space tutorial steps.
/// </summary>
public class TutorialHandlerWS
{
    private TutorialArrow _currentArrow;
    private TutorialArrowFactory.ArrowType _currentArrowType = TutorialArrowFactory.ArrowType.DEFAULT;

    /// <summary>
    /// Processes and then shows the tutorial step with intended arrow indicator format.
    /// </summary>
    /// <param name="wsStep">The World-Space step to show.</param>
    /// <returns><c>true</c> if the step has been shown successfully; otherwise, <c>false</c>.</returns>
    public bool Show(TutorialStepModelWS wsStep)
    {
        // Locate target
        string objectID = wsStep.FocusObject;
        GameObject primaryTarget = GameObject.Find(objectID);
        if (primaryTarget == null)
        {
            Debug.LogError("Could not locate primary target for WS Step in TutorialHandlerWS. Target: " + objectID);
            return false;
        }
        else
        {
            // Check if there is a more specific target
            string secondaryID = wsStep.ActualTarget;
            if (!string.IsNullOrEmpty(secondaryID))
            {
                Transform secondaryTarget = primaryTarget.transform.FindDeepChild(secondaryID);
                if (secondaryTarget != null)
                {
                    primaryTarget = secondaryTarget.gameObject;
                }
                else
                {
                    Debug.LogError("Could not find secondary target in TutorialHandlerWS. Target: " + secondaryID);
                }
            }
        }
        // Get Message
        string tmessage = wsStep.Message;
        if (string.IsNullOrEmpty(tmessage))
        {
            Debug.LogWarning("Message in WS Tutorial Step does not contain a message.");
        }

        // Get Arrow Type (probably default)
        TutorialArrowFactory.ArrowType arrowType = wsStep.ArrowType;

        //Get Arrow Offsets
        Vector3 positionOffset = wsStep.arrowPositionOffset;
        Vector3 rotationOffset = wsStep.arrowRotationOffset;

        // Finally, main call
        bool success = PointTo(primaryTarget, tmessage, wsStep.ArrowType, positionOffset, rotationOffset);
        return success;
    }

    private bool PointTo(
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

            _currentArrowType = arrowType;

            var factory = TutorialArrowFactory.Instance();
            _currentArrow = factory.CreateArrow(arrowType);
        }

        _currentArrow.PointTo(target, message, arrowPositionOffset, arrowRotationOffset);

        return true;
    }

    /// <summary>
    /// Removes indicators from the scene.
    /// </summary>
    public void Hide()
    {
        if (_currentArrow != null)
        {
            _currentArrow.Dissapear();
        }
    }
}
