using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

/// <summary>
/// @bibeg
/// Class for handling rotation and other manipulation on the 3d model
/// </summary>
public class MirageXRModelManipulation : MonoBehaviour, IMixedRealityGestureHandler<Vector3>
{

    private float RotateSpeed = 20f;
    /// <summary>
    /// Speed of interactive rotation via navigation gestures
    /// </summary>
    private float RotationFactor = 25f;

    /// <summary>
    /// hold the last transform.rotation
    /// </summary>
    private Quaternion lastRotation;

    /// <summary>
    /// value to turn off and on the rotation which didnt work!!
    /// </summary>
    [SerializeField] private bool rotatingEnabled = true;

    public void OnGestureStarted(InputEventData eventData)
    {
        //push the object into modal so that if you lose focus the object can still be rotated
        // commenting this out since the InputManager does not exist anymore in the MRTK v2
        //InputManager.Instance.PushModalInputHandler(gameObject);
        lastRotation = transform.rotation;
    }

    public void OnGestureCompleted(InputEventData eventData)
    {
        // commenting this out since the InputManager does not exist anymore in the MRTK v2
        //InputManager.Instance.PopModalInputHandler();
        //remove the component and add the drag component back
        TurnOffRotation();
    }

    public void OnGestureCompleted(InputEventData<Vector3> eventData)
    {
    }

    public void OnGestureCanceled(InputEventData eventData)
    {
        // commenting this out since the InputManager does not exist anymore in the MRTK v2
        //InputManager.Instance.PopModalInputHandler();
        TurnOffRotation();
    }

    public void OnGestureUpdated(InputEventData eventData)
    {
    }

    public void OnGestureUpdated(InputEventData<Vector3> eventData)
    {
        if (rotatingEnabled)
        {
            //calulate the new rotation based on the eventdata
            var rotation = new Quaternion(eventData.InputData.y * RotationFactor,
                eventData.InputData.x * RotationFactor,
                eventData.InputData.z * RotationFactor,
                0f);

            Rotate(rotation);
        }
    }

    void Rotate(Quaternion rotation)
    {
        gameObject.transform.rotation = Quaternion.Euler(
            new Vector3(lastRotation.x + rotation.x,
                 lastRotation.y + rotation.y,
                 lastRotation.z + rotation.z) * RotateSpeed);
    }

    /// <summary>
    /// 
    /// when the manipulation is completed or stopped revert the booleans
    /// </summary>
    void TurnOffRotation()
    {
        Destroy(gameObject.GetComponent<MirageXRModelManipulation>());
        //gameObject.GetComponent<HandDraggable>().IsDraggingEnabled = false
        //gameObject.GetComponent<MirageXRModelManipulation>().rotatingEnabled = true;
        gameObject.AddComponent<ObjectManipulator>();
    }
}
