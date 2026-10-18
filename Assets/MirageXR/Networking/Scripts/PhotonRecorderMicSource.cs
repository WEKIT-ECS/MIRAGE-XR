#if FUSION2
using System;
using LearningExperienceEngine.NewDataModel;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Connects Photon Voice Recorder to lib-lee's MicrophoneManager.
    /// Captures raw microphone frames via Photon's pre-processor pipeline without interrupting voice transmission.
    /// Also allows temporary release of the hardware microphone when muted.
    /// </summary>
    public class PhotonRecorderMicSource : MonoBehaviour, IMicrophoneSource, IProcessor<float>
    {
        private Recorder _recorder;
        private LocalVoiceAudio<float> _localVoice;
        private int _sampleRate = 24000;
        private int _channels = 1;
        private bool _isReleasedForSharedMic;

        public bool IsAvailable => _recorder != null && _recorder.RecordingEnabled && _recorder.TransmitEnabled && _localVoice != null && !_isReleasedForSharedMic;
        public int SampleRate => _sampleRate;
        public int Channels => _channels;

        public event Action<float[], int, int> OnFrameAvailable;

        public void Initialize(Recorder recorder)
        {
            _recorder = recorder;
        }

        // Unity message sent by Photon Recorder
        private void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
        {
            if (p.Voice is LocalVoiceAudio<float> voiceFloat)
            {
                _localVoice = voiceFloat;
                if (p.AudioDesc != null)
                {
                    _sampleRate = p.AudioDesc.SamplingRate;
                    _channels = p.AudioDesc.Channels;
                }
                else
                {
                    _sampleRate = (int)_recorder.SamplingRate;
                    _channels = 1;
                }

                _localVoice.AddPreProcessor(this);
                Debug.Log($"[PhotonRecorderMicSource] PreProcessor attached. SampleRate: {_sampleRate}, Channels: {_channels}");
            }
        }

        // Unity message sent by Photon Recorder
        private void PhotonVoiceRemoved()
        {
            if (_localVoice != null)
            {
                var voice = _localVoice;
                _localVoice = null;
                try
                {
                    voice.RemoveProcessor(this);
                }
                catch (Exception)
                {
                    // Ignore if voice is already disposed
                }
                Debug.Log("[PhotonRecorderMicSource] PreProcessor removed.");
            }
        }

        public float[] Process(float[] buf)
        {
            if (buf != null && buf.Length > 0 && OnFrameAvailable != null)
            {
                float[] copy = new float[buf.Length];
                Array.Copy(buf, copy, buf.Length);
                OnFrameAvailable.Invoke(copy, _sampleRate, _channels);
            }
            return buf;
        }

        public bool TryReleaseHardware()
        {
            if (_recorder == null) return false;

            if (_recorder.RecordingEnabled && (_localVoice == null || !_recorder.TransmitEnabled))
            {
                _isReleasedForSharedMic = true;
                _recorder.RecordingEnabled = false;
                Debug.Log("[PhotonRecorderMicSource] Released hardware microphone for content recording.");
                return true;
            }
            return false;
        }

        public void ReacquireHardware()
        {
            if (_isReleasedForSharedMic && _recorder != null)
            {
                _isReleasedForSharedMic = false;
                _recorder.RecordingEnabled = true;
                Debug.Log("[PhotonRecorderMicSource] Reacquired hardware microphone after content recording.");
            }
        }

        public void Dispose()
        {
            // Called by LocalVoiceFramed.Dispose() while iterating over its processors list.
            // Do NOT call RemoveProcessor(this) here, as that modifies the list being enumerated.
            _localVoice = null;
        }

        private void OnDestroy()
        {
            if (_localVoice != null)
            {
                PhotonVoiceRemoved();
            }
        }
    }
}
#endif
