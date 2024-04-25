using System.Collections;
using UnityEngine;
using TMPro;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using IBM.Watson.SpeechToText.V1;
using IBM.Watson.SpeechToText.V1.Model;
using System.IO;

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
    // The AudioClip of the speech
    AudioClip speechClip; 

    void Start()
    {
        _iamApikey = _audioEditView._iamApikey_();
        _serviceUrl = _audioEditView._serviceUrl_();
        
        StartCoroutine(CreateService());

        if(_audioEditView.SaveAndReturnAudioClipPath() != null)
        {
            string audioFileName = _audioEditView.SaveAndReturnAudioClipPath();
            soundPath = Path.Combine(audioFileName);
            StartCoroutine(ConvertAudioFileToText(soundPath)); 
        }
    }

    //The method runs asynchronously while creating an authenticator object
    //When successfully creates SpeechToTextService object and set the service url

    private IEnumerator CreateService()
    {
        var authenticator = new IamAuthenticator(apikey: _iamApikey);

        while (!authenticator.CanAuthenticate())
        {
            yield return null;
        }

        speechToText = new SpeechToTextService(authenticator);
        speechToText.SetServiceUrl(_serviceUrl);
    }

    //takes a the audio file path (audioFilePath) 
    //and converts the audio content to text using speech-to-text service.
    private IEnumerator ConvertAudioFileToText(string audioFilePath)
    {
        //Setting Up and Initial Checks
        _spinner.SetActive(true); 
        Debug.Log("ConvertAudioFileToText");

        transcribedSpeech.text = "";
        
        if (speechToText == null)
        {
            yield return StartCoroutine(CreateService());
        }

        //reading the audio file and storing the audio data
        byte[] audioBytes = File.ReadAllBytes(audioFilePath);
        speechClip = WaveFile.ParseWAV("myClip", audioBytes);

        //Sending Recognition Request
        SpeechRecognitionResults recognizeResponse = null;
        using (MemoryStream audioStream = new MemoryStream(audioBytes))
        {
            speechToText.Recognize(
                callback: (DetailedResponse<SpeechRecognitionResults> response, IBMError error) =>
                {
                    Debug.Log("The response is: " + response.Response);
                    Debug.Log("Converting speech to text...");
                    Log.Debug ("SpeechToTextServiceV1", "Recognize result: {0}", response.Response);
                    recognizeResponse = response.Result;
                },
                audio: audioStream,
                contentType: "audio/wav",
                timestamps: true
            );
        }

        //Waiting for Recognition Response
        while (recognizeResponse == null)
        {
            yield return null;
        }

        //Processing Recognized Text
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
        //Debug.Log("These are generated captions" + text);
    
        _spinner.SetActive(false);
    }

    //
    /// <summary>
    /// Returns the generated caption 
    /// </summary>
    public string GeneratedCaption()
    {
        return text;
    }
}
