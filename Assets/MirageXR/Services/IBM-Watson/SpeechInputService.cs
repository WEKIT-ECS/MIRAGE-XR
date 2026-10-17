/**
* Modified by Fridolin/Xinyu
*
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
#pragma warning disable 0649

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using IBM.Watson.SpeechToText.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using System;
using System.Collections.Generic;
using MirageXR;
using System.Linq;
using LearningExperienceEngine.NewDataModel;

public class SpeechInputService : MonoBehaviour
{
    [Space(10)]

    //[Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    //public string serviceUrl = "URL";

    [Tooltip("Text field to display the results of streaming.")]
    [SerializeField] private Text ResultsField;

    //[Header("IAM Authentication")]
    //[Tooltip("The IAM apikey.")]
    //public string iamApikey = "API";

    [Header("Parameters")]
    // https://www.ibm.com/watson/developercloud/speech-to-text/api/v1/curl.html?curl#get-model
    [Tooltip("The Model to use. This defaults to en-US_BroadbandModel")]
    [SerializeField] private string _recognizeModel;


    private SpeechOutputService myTTS;
    private DialogueService myDS;

    public delegate void CallbackStack(string text);
    public CallbackStack onInputReceived;

    //public Action onInputReceived;

    private IDisposable _audioSubscription;
    private readonly List<float> _samplesBuffer = new();
    private readonly object _bufferLock = new();

    //private AudioSource audioSrc;

    private SpeechToTextService _service;

    //string _testString = "<speak version=\"1.0\"><express-as type=\"Hi\">Hi How are you today!</express-as></speak>";

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());

        myDS = GetComponent<DialogueService>();
        //audioSrc = GetComponent<AudioSource>();
    }

    private IEnumerator CreateService()
    {
        /*

        if (string.IsNullOrEmpty(iamApikey))
        {
            throw new IBMException("Please provide IAM ApiKey for the service.");
        }

        //  Create credential and instantiate service
        Credentials credentials = null;

        //  Authenticate using iamApikey
        TokenOptions tokenOptions = new TokenOptions()
        {
            IamApiKey = iamApikey
        };

        credentials = new Credentials(tokenOptions, serviceUrl);

        //  Wait for tokendata
        while (!credentials.HasIamTokenData())
            yield return null;

        _service = new SpeechToTextService(credentials);

         */

        _service = new SpeechToTextService();

        while (!_service.Authenticator.CanAuthenticate()) yield return null; // .Credentials.HasIamTokenData()

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
                _service.RecognizeModel = (string.IsNullOrEmpty(_recognizeModel) ? "en-US_BroadbandModel" : _recognizeModel);
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
                _service.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _service.IsListening)
            {
                _service.StopListening();
            }
        }
    }

    public void StartRecording()
    {
        if (_audioSubscription != null) return;
        lock (_bufferLock)
        {
            _samplesBuffer.Clear();
        }
        if (RootObject.Instance != null && RootObject.Instance.LEE != null && RootObject.Instance.LEE.MicrophoneManager != null)
        {
            _audioSubscription = RootObject.Instance.LEE.MicrophoneManager.SubscribeRawAudio(OnRawAudioFrame);
        }
    }

    public void StopRecording()
    {
        _audioSubscription?.Dispose();
        _audioSubscription = null;
        lock (_bufferLock)
        {
            _samplesBuffer.Clear();
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("STT.OnError()", "Error! {0}", error);
    }

    private void OnRawAudioFrame(float[] data, int sampleRate, int channels)
    {
        if (_service == null || !_service.IsListening) return;

        float[] chunkToProcess = null;
        int chunkSampleRate = sampleRate;

        lock (_bufferLock)
        {
            if (channels <= 1)
            {
                _samplesBuffer.AddRange(data);
            }
            else
            {
                int count = data.Length / channels;
                for (int i = 0; i < count; i++)
                {
                    float sum = 0f;
                    int offset = i * channels;
                    for (int c = 0; c < channels; c++) sum += data[offset + c];
                    _samplesBuffer.Add(sum / channels);
                }
            }

            int targetSamples = sampleRate / 2;
            if (_samplesBuffer.Count >= targetSamples)
            {
                chunkToProcess = _samplesBuffer.GetRange(0, targetSamples).ToArray();
                _samplesBuffer.RemoveRange(0, targetSamples);
            }
        }

        if (chunkToProcess != null)
        {
            AudioData record = new AudioData();
            record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(chunkToProcess)), Mathf.Max(chunkToProcess));
            record.Clip = AudioClip.Create("Recording", chunkToProcess.Length, 1, chunkSampleRate, false);
            record.Clip.SetData(chunkToProcess, 0);

            _service.OnListen(record);
        }
    }

    private void OnDestroy()
    {
        StopRecording();
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    //Log.Debug("STT.OnRecognize(): ", text);
                    ResultsField.text = text;

                    if (res.final)
                    {

                        string _SpeechOutputString = alt.transcript;
                        //Debug.Log("STT: delivering recognised text to the callback stack");
                        onInputReceived?.Invoke(_SpeechOutputString);
                        Active = false; //stop Microphone from listening
                    }
                    //myTTS.Speak("Translating to Chinese");
                    //myTranslator.Translate(alt.transcript);

                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        //Log.Debug("STT.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("STT.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("STT.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("STT.OnRecognizeSpeaker()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }




}