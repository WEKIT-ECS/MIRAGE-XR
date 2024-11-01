using System.Collections;
using UnityEngine;
using TMPro;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Watson.SpeechToText.V1;
using IBM.Watson.SpeechToText.V1.Model;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Android;
using System;

public class AudioCaptionGenerator : MonoBehaviour
{
    [SerializeField] private TMP_Text transcribedSpeech;
    [SerializeField] private GameObject _spinner;
    [SerializeField] private AudioEditorView _audioEditView;

    private SpeechToTextService speechToText;
    private string soundPath;
    private string text = string.Empty;
    private string _serviceUrl;
    private string _iamApikey;

    private AudioSource audioSource;
    private byte[] audioBytes;
    private AudioClip speechClip;

    void Start()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        

        // Load API keys
        _iamApikey = _audioEditView.GetApiKey();
        _serviceUrl = _audioEditView.GetServiceUrl();

        yield return StartCoroutine(CreateService());

        string audioFileName = _audioEditView.SaveAndReturnAudioClipPath();
        if (!string.IsNullOrEmpty(audioFileName))
        {
            soundPath = audioFileName;
            yield return StartCoroutine(ConvertAudioFileToText(soundPath));
        }
    }



    private IEnumerator CreateService()
    {
        var authenticator = new IamAuthenticator(apikey: _iamApikey);

        while (!authenticator.CanAuthenticate())
        {
            Debug.LogWarning("Waiting for authenticator to be ready...");
            yield return null;
        }

        speechToText = new SpeechToTextService(authenticator);
        speechToText.SetServiceUrl(_serviceUrl);
    }

    private IEnumerator ConvertAudioFileToText(string audioFilePath)
    {
        // Setting Up and Initial Checks
        _spinner.SetActive(true);
        Debug.Log("ConvertAudioFileToText");

        transcribedSpeech.text = "";

        if (speechToText == null)
        {
            Debug.LogError("SpeechToText service is not initialized.");
            yield break;
        }

        // Verify file existence
        if (!File.Exists(audioFilePath))
        {
            Debug.LogError("Audio file not found at path: " + audioFilePath);
            _spinner.SetActive(false);
            yield break;
        }

        // Reading the audio file and storing the audio data
        byte[] audioBytes = null;
        try
        {
            audioBytes = File.ReadAllBytes(audioFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to read audio file: " + ex.Message);
            _spinner.SetActive(false);
            yield break;
        }

        // Sending Recognition Request
        SpeechRecognitionResults recognizeResponse = null;
        using (MemoryStream audioStream = new MemoryStream(audioBytes))
        {
            speechToText.Recognize(
                callback: (DetailedResponse<SpeechRecognitionResults> response, IBMError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("Error in Recognize: " + error.ToString());
                        recognizeResponse = new SpeechRecognitionResults(); // Avoid null reference
                        return;
                    }
                    else
                    {
                        Debug.Log("The response is: " + response.Response);
                        recognizeResponse = response.Result;
                    }
                },
                audio: audioStream,
                contentType: "audio/wav",
                timestamps: true
            );
        }

        // Waiting for Recognition Response
        while (recognizeResponse == null)
        {
            yield return null;
        }

        // Processing Recognized Text
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

        _spinner.SetActive(false);
    }

    /// <summary>
    /// Returns the generated caption 
    /// </summary>
    public string GeneratedCaption()
    {
        return text;
    }
}
