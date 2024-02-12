using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.MixedReality.Toolkit.Utilities;
using MirageXR;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class GhostRecorder
{
    public bool IsRecording => _isRecording;
    public GhostDataFrame LastFrame => _lastFrame;

    private readonly List<GhostDataFrame> _ghostFrames = new List<GhostDataFrame>();

    private GhostDataFrame _lastFrame;
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
    public static void ExportToFile(string filePath, List<GhostDataFrame> ghostDataFrames)
    {
        using (var file = File.Create(filePath))
        {
            var xmlSerializer = new XmlSerializer(typeof(List<GhostDataFrame>));
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
    public static bool TryLoadFromFile(string filePath, out List<GhostDataFrame> ghostDataFrames)
    {
        ghostDataFrames = null;

        if (!File.Exists(filePath))
        {
            return false;
        }

        using (var file = File.Open(filePath, FileMode.Open))
        using (TextReader textReader = new StreamReader(file))
        {
            var serializer = new XmlSerializer(typeof(List<GhostDataFrame>));
            ghostDataFrames = (List<GhostDataFrame>)serializer.Deserialize(textReader);
        }

        return ghostDataFrames != null && ghostDataFrames.Count != 0;
    }

    /// <summary>
    /// Start recording the position of the ghost
    /// </summary>
    /// <param name="anchor">Record anchor</param>
    /// <param name="camera">Main camera </param>
    /// <param name="cooldown">Time between capturing ghost position. The default is 'Time.fixedDeltaTime'</param>
    /// <exception cref="Exception">Throws an exception if called while recording is on.</exception>
    public void Start(Transform anchor, Transform camera, float? cooldown = null)
    {
        const int millisecondsInSecond = 1000;

        if (_isRecording)
        {
            throw new Exception("A new recording cannot be started because the previous one has not been completed.");
        }

        _cooldown = (int)((cooldown ?? Time.fixedDeltaTime) * millisecondsInSecond);
        _cancellationTokenSource = new CancellationTokenSource();
        _cameraTransform = camera;
        _anchor = anchor;

        _ghostFrames.Clear();
        _isRecording = true;

        StartAsync();
    }

    public List<GhostDataFrame> Stop()
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

        _lastFrame = new GhostDataFrame
        {
            head = CreateLocalPose(_anchor, GetHeadPosition(_cameraTransform), cameraRotation),
            rightHand = CreateLocalPose(_anchor, GetRightHandPosition(_cameraTransform), cameraRotation),
            leftHand = CreateLocalPose(_anchor, GetLeftHandPosition(_cameraTransform), cameraRotation),
            upperSpine = CreateLocalPose(_anchor, GetUpperSpinPosition(_cameraTransform), cameraRotation),
            lowerSpine = CreateLocalPose(_anchor, GetLowerSpinPosition(_cameraTransform), cameraRotation),
        };

        if (InputRayUtils.TryGetHandRay(Handedness.Right, out var rightHandRay))
        {
            var rotation = Quaternion.LookRotation(rightHandRay.direction, Vector3.up);
            _lastFrame.rightHand = CreateLocalPose(_anchor, rightHandRay.origin, rotation);
        }

        if (InputRayUtils.TryGetHandRay(Handedness.Left, out var leftHandRay))
        {
            var rotation = Quaternion.LookRotation(leftHandRay.direction, Vector3.up);
            _lastFrame.leftHand = CreateLocalPose(_anchor, leftHandRay.origin, rotation);
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