using UnityEngine;


public class GazeGestureManager : MonoBehaviour
{
    public GameObject annotationObject;

    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }

//    // Use this for initialization
//    void Start()
//    {
//        Instance = this;
//#if UNITY_WSA
//        // Set up a GestureRecognizer to detect Select gestures.
//        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
//        recognizer.TappedEvent += (source, tapCount, ray) =>
//        {
//            // Send an OnSelect message to the focused object and its ancestors.
//            if (FocusedObject != null)
//            {
//                FocusedObject.SendMessage("OnAnnotate", annotationObject);
//                //                FocusedObject.SendMessageUpwards("OnSelect");
//            }
//        };
//        recognizer.StartCapturingGestures();
//#endif
//    }

    // Update is called once per frame
//    void Update()
//    {
//        // Figure out which hologram is focused this frame.
//        GameObject oldFocusObject = FocusedObject;

//        // Do a raycast into the world based on the user's
//        // head position and orientation.
//        var headPosition = Camera.main.transform.position;
//        var gazeDirection = Camera.main.transform.forward;

//        RaycastHit hitInfo;

//        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
//        {
//            // If the raycast hit a hologram, use that as the focused object.
//            FocusedObject = hitInfo.collider.gameObject;
//        }
//        else
//        {
//            // If the raycast did not hit a hologram, clear the focused object.
//            FocusedObject = null;
//        }

//        // If the focused object changed this frame,
//        // start detecting fresh gestures again.
//        if (FocusedObject != oldFocusObject)
//        {
//#if UNITY_WSA
//            recognizer.CancelGestures();
//            recognizer.StartCapturingGestures();
//#endif
//        }
//    }
}