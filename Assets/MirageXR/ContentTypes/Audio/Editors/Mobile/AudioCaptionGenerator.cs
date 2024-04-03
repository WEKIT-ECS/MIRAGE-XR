using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using IBM.Watson.SpeechToText.V1;
using IBM.Watson.SpeechToText.V1.Model;
using System.IO;
using UnityEngine.UI;

public class AudioCaptionGenerator : MonoBehaviour
{
    private string _serviceUrl;
    private string _iamApikey;
    [SerializeField] private TMP_Text transcribedSpeech;
    [SerializeField] private Button convertAudioToTextButton;
    
    [SerializeField] private GameObject _spinner;
    // Make sure you have a reference to the AudioEditorView
    [SerializeField] private AudioEditorView _audioEditView;

    private SpeechToTextService speechToText;
    private string soundPath;
    private string text = string.Empty;

    private AudioSource audioSource;
    byte[] audioBytes;

    AudioClip speechClip; // The AudioClip of the speech

    void Start()
    {
        LoadKeysFromEnvFile(out var apiKey, out var serviceUrl);

        if (apiKey == null)
        {
            Debug.LogError($"Couldn't load 'apiKey' for {nameof(AudioCaptionGenerator)}");
            return;
        }

        if (serviceUrl == null)
        {
            Debug.LogError($"Couldn't load 'serviceUrl' for {nameof(AudioCaptionGenerator)}");
            return;
        }

        _iamApikey = apiKey;
        _serviceUrl = serviceUrl;

        Debug.Log("API Key: " + _iamApikey);
        Debug.Log("URL: " + _serviceUrl);

        StartCoroutine(CreateService());

        if(_audioEditView.SaveAndReturnAudioClipPath() != null)
        {
            string audioFileName = _audioEditView.SaveAndReturnAudioClipPath();
            soundPath = Path.Combine(audioFileName);
            convertAudioToTextButton.onClick.AddListener(delegate { StartCoroutine(ConvertAudioFileToText(soundPath)); });
        }
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

    private IEnumerator CreateService()
    {
        var authenticator = new IamAuthenticator(apikey: _iamApikey);

        while (!authenticator.CanAuthenticate())
            yield return null;

        speechToText = new SpeechToTextService(authenticator);
        speechToText.SetServiceUrl(_serviceUrl);
    }

    private IEnumerator ConvertAudioFileToText(string audioFilePath)
    {
        _spinner.SetActive(true); 
        Debug.Log("ConvertAudioFileToText");

        transcribedSpeech.text = "";

        if (speechToText == null)
        {
            yield return StartCoroutine(CreateService());
        }

        byte[] audioBytes = File.ReadAllBytes(audioFilePath);
        speechClip = WaveFile.ParseWAV("myClip", audioBytes);

        SpeechRecognitionResults recognizeResponse = null;

        using (MemoryStream audioStream = new MemoryStream(audioBytes))
        {
            speechToText.Recognize(
                callback: (DetailedResponse<SpeechRecognitionResults> response, IBMError error) =>
                {
                    Debug.Log("Converting speech to text...");
                    Log.Debug("SpeechToTextServiceV1", "Recognize result: {0}", response.Response);
                    Debug.Log("The response is: " + response.Response);
                    recognizeResponse = response.Result;
                },
                audio: audioStream,
                contentType: "audio/wav",
                timestamps: true
            );
        }

        while (recognizeResponse == null)
        {
            yield return null;
        }

        transcribedSpeech.text = "";
        foreach (var res in recognizeResponse.Results)
        {
            foreach (var alt in res.Alternatives)
            {
                Debug.Log(alt.Transcript);
                transcribedSpeech.text += alt.Transcript + "\n";
            }
        }
        text = transcribedSpeech.text;
        Debug.Log("These are generated captions" + text);
    
        _spinner.SetActive(false);
    }

    public string GeneratedCaption()
    {
        return text;
    }
}
