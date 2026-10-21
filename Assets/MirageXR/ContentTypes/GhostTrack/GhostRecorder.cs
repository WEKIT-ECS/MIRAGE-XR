using LearningExperienceEngine;
using MirageXR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.XR.Hands;

public class GhostRecorder
{
    public bool IsRecording => _isRecording;
    public LearningExperienceEngine.GhostDataFrame LastFrame => _lastFrame;

    private HandTrackingManager _handTrackingManager;
    private readonly List<LearningExperienceEngine.GhostDataFrame> _ghostFrames = new List<LearningExperienceEngine.GhostDataFrame>();

    private LearningExperienceEngine.GhostDataFrame _lastFrame;
    private CancellationTokenSource _cancellationTokenSource;
    private Transform _anchor;
    private Transform _cameraTransform;
    private int _cooldown;
    private bool _isRecording;

    /// <summary>
    /// Write the data in an .xml file and save it locally.
    /// </summary>
    /// <param name="filePath">Desired name for the to be saved file.</param>
    /// <param name="ghostDataFrames">Record data that is to be saved.</param>
    public static void ExportToFile(string filePath, List<LearningExperienceEngine.GhostDataFrame> ghostDataFrames)
    {
        using (var file = File.Create(filePath))
        {
            var xmlSerializer = new XmlSerializer(typeof(List<LearningExperienceEngine.GhostDataFrame>));
            using (TextWriter textWriter = new StreamWriter(file))
            {
                xmlSerializer.Serialize(textWriter, ghostDataFrames);
            }
        }

        Debug.Log($"saved ghost track file: {filePath}");
    }

    /// <summary>
    /// Load the data from an .xml file
    /// </summary>
    /// <param name="filePath">Desired name for the to be saved file.</param>
    /// <param name="ghostDataFrames">Record data that is to be saved.</param>
    public static bool TryLoadFromFile(string filePath, out List<LearningExperienceEngine.GhostDataFrame> ghostDataFrames)
    {
        ghostDataFrames = null;

        if (!File.Exists(filePath))
        {
            return false;
        }

        using (var file = File.OpenRead(filePath))
        {
            var xmlSerializer = new XmlSerializer(typeof(List<LearningExperienceEngine.GhostDataFrame>));
            ghostDataFrames = (List<LearningExperienceEngine.GhostDataFrame>)xmlSerializer.Deserialize(file);
        }

        return true;
    }

    /// <summary>
    /// Call this function to start recording.
    /// </summary>
    /// <param name="camera">Target camera that is going to be recorded.</param>
    /// <param name="anchor">The parent of the recorded object. Transform values of the record are going to be local to this object.</param>
    /// <param name="cooldown">Cooldown between recording frames in milliseconds.</param>
    public void Start(Transform camera, Transform anchor, int cooldown = 50)
    {
        if (_isRecording)
        {
            throw new Exception("The recording cannot be started because the recording has already been started.");
        }

        _cooldown = cooldown;
        _cancellationTokenSource = new CancellationTokenSource();
        _cameraTransform = camera;
        _anchor = anchor;

        _ghostFrames.Clear();
        _isRecording = true;

        _handTrackingManager = UnityEngine.Object.FindObjectOfType<HandTrackingManager>();

        StartAsync();
    }

    public List<LearningExperienceEngine.GhostDataFrame> Stop()
    {
        if (!_isRecording)
        {
            throw new Exception("The recording cannot be stopped because the recording has not been started.");
        }

        _isRecording = false;
        _cancellationTokenSource.Cancel();
        return _ghostFrames;
    }

    private async void StartAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            RecordFrame();
            try
            {
                await Task.Delay(_cooldown, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException) { /*hide exception*/ }
        }
    }

    private void RecordFrame()
    {
        if (!_isRecording)
        {
            return;
        }

        var cameraRotation = _cameraTransform.rotation;

        _lastFrame = new LearningExperienceEngine.GhostDataFrame
        {
            head = CreateLocalPose(_anchor, GetHeadPosition(_cameraTransform), cameraRotation),
            rightHand = CreateLocalPose(_anchor, GetRightHandPosition(_cameraTransform), cameraRotation),
            leftHand = CreateLocalPose(_anchor, GetLeftHandPosition(_cameraTransform), cameraRotation),
            upperSpine = CreateLocalPose(_anchor, GetUpperSpinPosition(_cameraTransform), cameraRotation),
            lowerSpine = CreateLocalPose(_anchor, GetLowerSpinPosition(_cameraTransform), cameraRotation),
        };

        var zeroPose = new Pose { position = Vector3.zero, rotation = Quaternion.identity };
        _lastFrame.rightThumbTip = zeroPose;
        _lastFrame.rightIndexTip = zeroPose;
        _lastFrame.rightMiddleTip = zeroPose;
        _lastFrame.rightRingTip = zeroPose;
        _lastFrame.rightPinkyTip = zeroPose;
        _lastFrame.leftThumbTip = zeroPose;
        _lastFrame.leftIndexTip = zeroPose;
        _lastFrame.leftMiddleTip = zeroPose;
        _lastFrame.leftRingTip = zeroPose;
        _lastFrame.leftPinkyTip = zeroPose;

        if (_handTrackingManager != null)
        {
            if (_handTrackingManager.RightHandData.IsTracked)
            {
                var joints = _handTrackingManager.RightHandData.JointPoses;
                if (joints.TryGetValue(XRHandJointID.Palm, out var palmPose))
                {
                    _lastFrame.rightHand = CreateLocalPose(_anchor, palmPose.position, palmPose.rotation);
                }
                if (joints.TryGetValue(XRHandJointID.ThumbTip, out var tip)) _lastFrame.rightThumbTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.IndexTip, out tip)) _lastFrame.rightIndexTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.MiddleTip, out tip)) _lastFrame.rightMiddleTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.RingTip, out tip)) _lastFrame.rightRingTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.LittleTip, out tip)) _lastFrame.rightPinkyTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
            }

            if (_handTrackingManager.LeftHandData.IsTracked)
            {
                var joints = _handTrackingManager.LeftHandData.JointPoses;
                if (joints.TryGetValue(XRHandJointID.Palm, out var palmPose))
                {
                    _lastFrame.leftHand = CreateLocalPose(_anchor, palmPose.position, palmPose.rotation);
                }
                if (joints.TryGetValue(XRHandJointID.ThumbTip, out var tip)) _lastFrame.leftThumbTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.IndexTip, out tip)) _lastFrame.leftIndexTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.MiddleTip, out tip)) _lastFrame.leftMiddleTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.RingTip, out tip)) _lastFrame.leftRingTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
                if (joints.TryGetValue(XRHandJointID.LittleTip, out tip)) _lastFrame.leftPinkyTip = CreateLocalPose(_anchor, tip.position, tip.rotation);
            }
        }

        _ghostFrames.Add(_lastFrame);
    }

    private static Pose CreateLocalPose(Transform anchor, Vector3 position, Quaternion rotation)
    {
        return new Pose
        {
            position = anchor.InverseTransformPoint(position),
            rotation = Quaternion.Inverse(anchor.localRotation) * rotation,
        };
    }

    private static Vector3 GetHeadPosition(Transform camera)
    {
        return camera.position;
    }

    private static Vector3 GetRightHandPosition(Transform camera)
    {
        return camera.position + (camera.forward * 0.15f) + (camera.right * 0.35f) + (camera.up * -0.35f);
    }

    private static Vector3 GetLeftHandPosition(Transform camera)
    {
        return camera.position + (camera.forward * 0.15f) + (camera.right * -0.35f) + (camera.up * -0.35f);
    }

    private static Vector3 GetUpperSpinPosition(Transform camera)
    {
        return camera.position + (camera.up * -0.2f);
    }

    private static Vector3 GetLowerSpinPosition(Transform camera)
    {
        return camera.position + (camera.up * -0.45f);
    }
}