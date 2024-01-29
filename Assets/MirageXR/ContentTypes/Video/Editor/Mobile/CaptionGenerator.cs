using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using IBM.Watson.SpeechToText.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using TMPro;

public class CaptionGenerator : MonoBehaviour
{
    private const int RECORDING_BUFFER_SIZE = 1;
    private const int RECORDING_HZ = 22050;
    
    [SerializeField] private TMP_Text _resultsField;
    [SerializeField] private Button _stopButton;

    private string _serviceUrl;
    private string _iamApikey;
    private string _recognizeModel;
    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private SpeechToTextService _service;
    private string _accumulatedText = string.Empty;

    private void Start()
    {
        _stopButton.onClick.AddListener(StopService);
        LoadKeysFromEnvFile(out var apiKey, out var serviceUrl);
        
        if (apiKey == null)
        {
            Debug.LogError($"Couldn't load 'apiKey' for {nameof(CaptionGenerator)}");
            return;
        }

        if (serviceUrl == null)
        {
            Debug.LogError($"Couldn't load 'serviceUrl' for {nameof(CaptionGenerator)}");
            return;
        }

        _iamApikey = apiKey;
        _serviceUrl = serviceUrl;

        Debug.Log("API Key: " + _iamApikey);
        Debug.Log("URL: " + _serviceUrl);
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    private static void LoadKeysFromEnvFile(out string apiKey, out string serviceUrl)
    {
        const string ibmFileName = "caption-ibm-credentials";
        const string speechToTextApikey = "SPEECH_TO_TEXT_IAM_APIKEY";
        const string speechToTextURL = "SPEECH_TO_TEXT_URL";

        apiKey = null;
        serviceUrl = null;

        var ibmCredentials = Resources.Load(ibmFileName) as TextAsset;
        if (ibmCredentials == null)
        {
            Debug.LogError($"'{ibmFileName}' file not found");
            return;
        }

        using var sr = new StringReader(ibmCredentials.text);
        while (sr.ReadLine() is { } line)
        {
            var split = line.Split('=');
            if (split.Length != 2)
            {
                continue;
            }

            if (split[0] == speechToTextApikey)
            {
                apiKey = split[1].Trim();
            }

            if (split[0] == speechToTextURL)
            {
                serviceUrl = split[1].Trim();
            }
        }
    }

    public void StopService()
    {
        if (_service != null && _service.IsListening)
        {
            _service.StopListening();
            StopRecording();
            Debug.Log("Service stopped");
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

        var authenticator = new IamAuthenticator(apikey: _iamApikey);

        while (!authenticator.CanAuthenticate())
        {
            yield return null;
        }

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
        get => _service.IsListening;
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
        _recording = Microphone.Start(_microphoneID, true, RECORDING_BUFFER_SIZE, RECORDING_HZ);
        yield return null;

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        var bFirstBlock = true;
        var midPoint = _recording.samples / 2;

        while (_recordingRoutine != 0 && _recording != null)
        {
            var writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");
                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint) || (!bFirstBlock && writePos < midPoint))
            {
                var samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                var record = new AudioData
                {
                    MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples)),
                    Clip = AudioClip.Create("Recording", midPoint, _recording.channels, RECORDING_HZ, false)
                };
                record.Clip.SetData(samples, 0);

                _service.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                var remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                var timeRemaining = remaining / (float)RECORDING_HZ;

                yield return new WaitForSeconds(timeRemaining);
            }
        }
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        var finalText = string.Empty;
        if (result == null || result.results.Length <= 0)
        {
            return;
        }

        foreach (var res in result.results)
        {
            foreach (var alt in res.alternatives)
            {
                var text = $"{alt.transcript}\n";
                Log.Debug("ExampleStreaming.OnRecognize()", text);

                if (res.final)
                {
                    finalText = text;
                    _resultsField.text = finalText;
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

        _accumulatedText += finalText;
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (var labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognizeSpeaker()", "speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence);
            }
        }
    }

    public string AllGeneratedCaptions()
    {
        return _accumulatedText;
    }
}