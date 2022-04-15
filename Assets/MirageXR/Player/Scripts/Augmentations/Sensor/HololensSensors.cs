using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace MirageXR
{

    public class HololensSensors : SensorBase //TODO: now is useless
    {
        //Reference to the text that shall display the realtime data.
        private Text UIText;
        private RaycastHit hitInfo;

        private Vector3 handPos1;
        private Vector3 handPos2;


        private void Start()
        {
            this.WarmUp();
        }

        public override void FixedFrameUpdate()
        {
            var cameraTransform = Camera.main.transform;
            
            currentSensorDataFrame = new SensorDataFrame();

            //Check if the raycast from the user's head in the direction of his gaze hit an object.
            currentSensorDataFrame.CastHit = Physics.Raycast(currentSensorDataFrame.HeadPosition, currentSensorDataFrame.GazeDirection, out hitInfo);

            // add timestamp

            //Get user's head position
            currentSensorDataFrame.HeadPosition = cameraTransform.position;

            // Get user's gaze direction
            currentSensorDataFrame.GazeDirection = Camera.main.transform.forward;

            // Get hands' positions (if visible)
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Left, out MixedRealityPose leftPose))
            {
                currentSensorDataFrame.HandPosition1 = leftPose.Position;
            }
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Right, out MixedRealityPose rightPose))
            {
                currentSensorDataFrame.HandPosition2 = rightPose.Position;
            }

            // call base to execute callback stack for listeners
            base.FixedFrameUpdate();

        } // Update()

    } // class

} // namespace