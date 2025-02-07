using System.Collections;
using UnityEngine;
using TMPro;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication;
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

   // private SpeechToTextService speechToText;
    private string soundPath;
    private string text = string.Empty;

    private void Start()
    {
        StartCoroutine(Initialize());
        
    }

    private IEnumerator Initialize()
    {
        string audioFileName = _audioEditView.SaveAndReturnAudioClipPath();

        yield return StartCoroutine(CreateService());

        // If valid audio, process it
        if (!string.IsNullOrEmpty(audioFileName))
        {
            soundPath = audioFileName;
            yield return StartCoroutine(ConvertAudioFileToText(soundPath));
        }
    }

    private IEnumerator CreateService()
    {
        
        while (!_audioEditView.speechToText.Authenticator.CanAuthenticate())
        {
            Debug.LogWarning("Waiting for SpeechToTextService Authenticator to be ready...");
            yield return null;
        }

        yield break;
    }

    private IEnumerator ConvertAudioFileToText(string audioFilePath)
    {
        _spinner.SetActive(true);
        Debug.Log("ConvertAudioFileToText");
        transcribedSpeech.text = "";

        if (_audioEditView.speechToText == null)
        {
            Debug.LogError("SpeechToText service is not initialized.");
            yield break;
        }

        if (!File.Exists(audioFilePath))
        {
            Debug.LogError("Audio file not found: " + audioFilePath);
            _spinner.SetActive(false);
            yield break;
        }

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

        SpeechRecognitionResults recognizeResponse = null;
        using (MemoryStream audioStream = new MemoryStream(audioBytes))
        {
            _audioEditView.speechToText.Recognize(
                callback: (DetailedResponse<SpeechRecognitionResults> response, IBMError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("Error in Recognize: " + error.ToString());
                        // To avoid a null reference, set an empty object on error
                        recognizeResponse = new SpeechRecognitionResults();
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

        while (recognizeResponse == null)
        {
            yield return null;
        }

        transcribedSpeech.text = "";
        foreach (var result in recognizeResponse.Results)
        {
            foreach (var alt in result.Alternatives)
            {
                Debug.Log(alt.Transcript);
                transcribedSpeech.text += alt.Transcript + "\n";
            }
        }

        text = transcribedSpeech.text;
        _spinner.SetActive(false);
    }

    public string GeneratedCaption()
    {
        return text;
    }
}
