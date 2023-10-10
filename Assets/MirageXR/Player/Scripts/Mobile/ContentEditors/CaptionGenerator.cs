#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using IBM.Watson.SpeechToText.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using TMPro;

namespace IBM.Watsson.Examples
{
    public class CaptionGenerator : MonoBehaviour
    {
        #region PLEASE SET THESE VARIABLES IN THE INSPECTOR

        private string _serviceUrl;
        [Tooltip("Text field to display the results of streaming.")]
        [SerializeField]
        private TMP_Text ResultsField;
        private string _iamApikey;
        private string _recognizeModel;

        #endregion

        private int _recordingRoutine = 0;
        private string _microphoneID = null;
        private AudioClip _recording = null;
        private int _recordingBufferSize = 1;
        private int _recordingHZ = 22050;
        private SpeechToTextService _service;
        private string accumulatedText = "";

        void Start()
        {
            LoadKeysFromEnvFile();
            Debug.Log("API Key: " + _iamApikey);
            Debug.Log("URL: " + _serviceUrl);
            LogSystem.InstallDefaultReactors();
            Runnable.Run(CreateService());
        }

        private void LoadKeysFromEnvFile()
        {
            string pathToEnvFile = Application.dataPath + "/../ibm-credentials.env";

            if (File.Exists(pathToEnvFile))
            {
                var envVariables = ReadEnvFile(pathToEnvFile);

                if (envVariables.TryGetValue("SPEECH_TO_TEXT_IAM_APIKEY", out string apiKey))
                {
                    _iamApikey = apiKey;
                }
                else
                {
                    Debug.LogError("SPEECH_TO_TEXT_IAM_APIKEY not found in keys.env");
                }

                if (envVariables.TryGetValue("SPEECH_TO_TEXT_URL", out string serviceUrl))
                {
                    _serviceUrl = serviceUrl;
                }
                else
                {
                    Debug.LogError("SPEECH_TO_TEXT_URL not found in keys.env");
                }
            }
            else
            {
                Debug.LogError("keys.env file not found");
            }
        }

        private Dictionary<string, string> ReadEnvFile(string pathToEnvFile)
        {
            var envVariables = new Dictionary<string, string>();
            var lines = File.ReadAllLines(pathToEnvFile);

            foreach (var line in lines)
            {
                var split = line.Split('=');
                if (split.Length == 2)
                {
                    envVariables[split[0].Trim()] = split[1].Trim();
                }
            }

            return envVariables;
        }

        private IEnumerator CreateService()
        {
            if (string.IsNullOrEmpty(_iamApikey))
            {
                throw new IBMException("Please provide IAM ApiKey for the service.");
            }

            IamAuthenticator authenticator = new IamAuthenticator(apikey: _iamApikey);

            while (!authenticator.CanAuthenticate())
                yield return null;

            _service = new SpeechToTextService(authenticator);
            if (!string.IsNullOrEmpty(_serviceUrl))
            {
                _service.SetServiceUrl(_serviceUrl);
            }
            _service.StreamMultipart = true;

            Active = true;
            StartRecording();
        }

        public bool Active
        {
            get { return _service.IsListening; }
            set
            {
                if (value && !_service.IsListening)
                {
                    _service.RecognizeModel = string.IsNullOrEmpty(_recognizeModel) ? "en-US_BroadbandModel" : _recognizeModel;
                    _service.DetectSilence = true;
                    _service.EnableWordConfidence = true;
                    _service.EnableTimestamps = true;
                    _service.SilenceThreshold = 0.01f;
                    _service.MaxAlternatives = 1;
                    _service.EnableInterimResults = true;
                    _service.OnError = OnError;
                    _service.InactivityTimeout = -1;
                    _service.ProfanityFilter = false;
                    _service.SmartFormatting = true;
                    _service.SpeakerLabels = false;
                    _service.WordAlternativesThreshold = null;
                    _service.EndOfPhraseSilenceTime = null;
                    _service.StartListening(OnRecognize, OnRecognizeSpeaker);
                }
                else if (!value && _service.IsListening)
                {
                    _service.StopListening();
                }
            }
        }

        private void StartRecording()
        {
            if (_recordingRoutine == 0)
            {
                UnityObjectUtil.StartDestroyQueue();
                _recordingRoutine = Runnable.Run(RecordingHandler());
            }
        }

        private void StopRecording()
        {
            if (_recordingRoutine != 0)
            {
                Microphone.End(_microphoneID);
                Runnable.Stop(_recordingRoutine);
                _recordingRoutine = 0;
            }
        }

        private void OnError(string error)
        {
            Active = false;
            Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
        }

        private IEnumerator RecordingHandler()
        {
            Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
            _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
            yield return null;

            if (_recording == null)
            {
                StopRecording();
                yield break;
            }

            bool bFirstBlock = true;
            int midPoint = _recording.samples / 2;
            float[] samples = null;

            while (_recordingRoutine != 0 && _recording != null)
            {
                int writePos = Microphone.GetPosition(_microphoneID);
                if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
                {
                    Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");
                    StopRecording();
                    yield break;
                }

                if ((bFirstBlock && writePos >= midPoint) || (!bFirstBlock && writePos < midPoint))
                {
                    samples = new float[midPoint];
                    _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                    AudioData record = new AudioData
                    {
                        MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples)),
                        Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false)
                    };
                    record.Clip.SetData(samples, 0);

                    _service.OnListen(record);

                    bFirstBlock = !bFirstBlock;
                }
                else
                {
                    int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                    float timeRemaining = (float)remaining / (float)_recordingHZ;

                    yield return new WaitForSeconds(timeRemaining);
                }
            }
            yield break;
        }

        private void OnRecognize(SpeechRecognitionEvent result)
        {
            string finalText = "";
            if (result != null && result.results.Length > 0)
            {
                foreach (var res in result.results)
                {
                    foreach (var alt in res.alternatives)
                    {
                    string text = $"{alt.transcript}\n";
                    Log.Debug("ExampleStreaming.OnRecognize()", text);

                    if (res.final)
                    {
                    
                    finalText = text;
                    ResultsField.text = finalText;
                    }
                       
                    }

                    if (res.keywords_result?.keyword != null)
                    {
                        foreach (var keyword in res.keywords_result.keyword)
                        {
                            Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                        }
                    }

                    if (res.word_alternatives != null)
                    {
                        foreach (var wordAlternative in res.word_alternatives)
                        {
                            Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                            foreach (var alternative in wordAlternative.alternatives)
                            {
                                Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                            }
                        }
                    }
                }
                
                accumulatedText += finalText;
                finalText = "";  // reset the temporary variable
            }
        }

        private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
        {
            if (result != null)
            {
                foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
                {
                    Log.Debug("ExampleStreaming.OnRecognizeSpeaker()", "speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence);
                }
            }
        }

        public string AllGeneratedCaptions()
        {
            return accumulatedText;
        }
    }
}
