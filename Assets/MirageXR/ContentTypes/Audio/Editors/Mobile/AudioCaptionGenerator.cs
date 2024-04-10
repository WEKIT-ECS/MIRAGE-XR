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
   // [SerializeField] private Button convertAudioToTextButton;
    
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
        _iamApikey = _audioEditView._iamApikey_();
        _serviceUrl = _audioEditView._serviceUrl_();
         

    StartCoroutine(CreateService());

        if(_audioEditView.SaveAndReturnAudioClipPath() != null)
        {
            string audioFileName = _audioEditView.SaveAndReturnAudioClipPath();
            soundPath = Path.Combine(audioFileName);
            //convertAudioToTextButton.onClick.AddListener(delegate { 
            StartCoroutine(ConvertAudioFileToText(soundPath)); 
            //});
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
