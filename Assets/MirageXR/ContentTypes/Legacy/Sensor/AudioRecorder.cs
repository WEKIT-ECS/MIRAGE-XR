using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public static class AudioRecorder
    {
        private const string RECORD_NAME = "record";
        private const int CYCLE_RECORDING_TIME = 60;
        private const int DEFAULT_FREQUENCY = 96000;

        private static AudioClip _audioClip;
        private static readonly List<float> _buffer = new List<float>();
        private static int _maxFrequency;
        private static CancellationTokenSource cancellationTokenSource;
        private static string _device;
        private static string _recordName;
        public static bool IsRecording => Microphone.IsRecording(_device);

        public static string[] GetRecordingDevices()
        {
            try
            {
                return Microphone.devices;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void SetRecordingDevice(string recordingDevice)
        {
            if (IsRecording)
            {
                Stop();
            }

            var devices = GetRecordingDevices();
            if (devices == null)
            {
                _device = null;
                return;
            }
            if (devices.Contains(recordingDevice))
            {
                _device = recordingDevice;
            }
            else
            {
                throw new ArgumentException($"Device {recordingDevice} does not exist");
            }
        }

        public static void Start(string recordName = RECORD_NAME)
        {
            _recordName = recordName;

            if (IsRecording)
            {
                Stop();
            }

            if (_device == null)
            {
                var devices = GetRecordingDevices();
                _device = devices?.First();
            }

            Microphone.GetDeviceCaps(_device, out _, out _maxFrequency);
            if (_maxFrequency == 0) _maxFrequency = DEFAULT_FREQUENCY;
            _buffer.Clear();
            _audioClip = null;
            StartRecord(CYCLE_RECORDING_TIME);
        }

        public static void Pause()
        {
            if (!IsRecording) return;

            var lastTime = Microphone.GetPosition(_device);
            if (lastTime == 0) return;

            cancellationTokenSource.Cancel();
            cancellationTokenSource = null;
            var samples = new float[_audioClip.samples];
            _audioClip.GetData(samples, 0);
            var clipSamples = new float[lastTime];
            Array.Copy(samples, clipSamples, clipSamples.Length - 1);
            _buffer.AddRange(clipSamples);
            Microphone.End(_device);
            _audioClip = null;
        }

        public static void Resume()
        {
            if (IsRecording || _buffer.Count == 0) return;

            StartRecord(CYCLE_RECORDING_TIME);
        }

        public static AudioClip Stop()
        {
            Pause();
            _audioClip = null;
            var audioClip = AudioClip.Create(_recordName, _buffer.Count, 1, _maxFrequency, false);
            audioClip.SetData(_buffer.ToArray(), 0);
            return audioClip;
        }

        private static async void StartWaitForRecordCircle(int timeInSeconds, CancellationToken token)
        {
            while (IsRecording)
            {
                try
                {
                    await Task.Delay(timeInSeconds * 1000, token);
                    var samples = new float[_audioClip.samples];
                    _audioClip.GetData(samples, 0);
                    _buffer.AddRange(samples);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private static void StartRecord(int recordingTime)
        {
            _audioClip = Microphone.Start(_device, true, recordingTime, _maxFrequency);
            cancellationTokenSource = new CancellationTokenSource();
            StartWaitForRecordCircle(recordingTime, cancellationTokenSource.Token);
        }
    }
}