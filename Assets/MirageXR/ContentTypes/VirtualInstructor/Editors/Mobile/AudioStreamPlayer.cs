using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Threading.Tasks;
using MirageXR;
using TMPro;

/// <summary>
/// Represents an audio stream player for playing audio clips.
/// </summary>
public class AudioStreamPlayer : MonoBehaviour
{
    /// <summary>
    /// The AudioSource used for playing audio streams.
    /// </summary>
    private AudioSource _audioSource;
    /// <summary>
    /// Represents a progress slider control used for audio streaming.
    /// </summary>
    private Slider _progressSlider;

    /// <summary>
    /// The text component used to display the current time of the audio clip being played.
    /// </summary>
    private TMP_Text _currentTimeText;
    private TMP_Text _durationText;

    /// <summary>
    /// Represents a play button used for audio streaming.
    /// </summary>
    private Button _playButton;
    /// <summary>
    /// Reference to the forward button in the UI.
    /// </summary>
    private Button _forward;

    /// <summary>
    /// Reference to the backward button in the UI.
    /// </summary>
    private Button _backward;

    /// <summary>
    /// Represents the game object for the play button in the UI.
    /// </summary>
    public GameObject play;
    public GameObject pause;


    /// <summary>
    /// Represents an AI model for the AiServices.
    /// </summary>
    private AIModel _model;
    private TMP_Text[] _textComponents;

    /// <summary>
    /// An array of Button objects representing the interactive buttons in the AudioStreamPlayer script. 
    /// </summary>
    private Button[] _buttons;

    /// <summary>
    /// Represents the current playing state of the AudioStreamPlayer.
    /// </summary>
    private bool _isPlaying;


    /// <summary>
    /// Initializes the AudioStreamPlayer by assigning references to its required components.
    /// </summary>
    void Awake()
    {
        
        _audioSource = GetComponent<AudioSource>();
        _progressSlider = gameObject.GetComponentInChildren<Slider>(); // ist null !
        _textComponents = gameObject.GetComponentsInChildren<TMP_Text>();
        _currentTimeText = _textComponents[1];
        _durationText = _textComponents[2];
        _buttons = gameObject.GetComponentsInChildren<Button>();
        _backward = _buttons[1];
        _playButton = _buttons[2];
        _forward = _buttons[3];
    }


    /// <summary>
    /// Sets up the AudioStreamPlayer component with the specified AIModel.
    /// </summary>
    /// <param name="model">The AIModel to use for setup.</param>
    public async void Setup(AIModel model)
    {
        _model = model;
        if (_textComponents.Length == 3)
        {
            _textComponents[0].text = _model.Name;
            _currentTimeText.text = "0.00";
            _audioSource.clip = await LoadAudioAsync();
            play.gameObject.SetActive(true);
            _playButton.onClick.AddListener(PlayAudio);
            _backward.onClick.AddListener(MoveBackward);
            _forward.onClick.AddListener(MoveForward);
            _progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
            _durationText.text = _audioSource.clip.length.ToString("F2", CultureInfo.CurrentCulture);
        }
    }

    /// <summary>
    /// Moves the audio playback forward by 10% of the total duration.
    /// </summary>
    private void MoveForward()
    {
        if (_audioSource != null)
        {
            float newTime = _audioSource.time + (_audioSource.clip.length * 0.1f);
            _audioSource.time = Mathf.Clamp(newTime, 0, _audioSource.clip.length);
            _audioSource.Play();
        }
    }

    /// <summary>
    /// Moves the audio playback position backward by 10% of the total duration.
    /// </summary>
    private void MoveBackward()
    {
        if (_audioSource != null)
        {
            float newTime = _audioSource.time - (_audioSource.clip.length * 0.1f);
            _audioSource.time = Mathf.Clamp(newTime, 0, _audioSource.clip.length);
            _audioSource.Play();
        }
    }

    /// <summary>
    /// Loads an audio clip asynchronously.
    /// </summary>
    /// <returns>The loaded audio clip.</returns>
    /// <remarks>
    /// This method is used to load an audio clip asynchronously. It generates a text message
    /// based on the AI model's name and passes it to the AIManager's ConvertTextToSpeechAsync method
    /// along with the model's API name. The method waits for the audio clip to be retrieved and returns it.
    /// </remarks>
    private async Task<AudioClip> LoadAudioAsync()
    {
        pause.gameObject.SetActive(false);
        play.gameObject.SetActive(false);
        var massage = "Hi I am " + _model.Name;
        AudioClip clip = await RootObject.Instance.aiManager.ConvertTextToSpeechAsync(massage, _model.ApiName);
        return clip;
    }


    /// <summary>
    /// Plays the audio clip attached to the audio source. If there is no audio clip attached or the audio source is already playing, this method does nothing.
    /// </summary>
    /// <param name="/*START_USER_CODE*/args/*END_USER_CODE*/">Optional arguments.</param>
    private void PlayAudio()
    {
        UnityEngine.Debug.Log("PlayAudio");
        if (_audioSource.clip != null)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }

            _audioSource.mute = false;
            _audioSource.volume = 1.0f;
            _audioSource.Play();
        }
    }

    /// <summary>
    /// Called when the value of the Slider component attached to the AudioStreamPlayer changes.
    /// </summary>
    /// <param name="value">The new value of the Slider component.</param>
    void OnSliderValueChanged(float value)
    {
        _audioSource.time = (value / 100) * _audioSource.clip.length;
    }


    /// <summary>
    /// Updates the audio stream player.
    /// </summary>
    void Update()
    {
        if (_audioSource.isPlaying)
        {
            _progressSlider.value = (_audioSource.time / _audioSource.clip.length) * 100;
            _currentTimeText.text = _audioSource.time.ToString("F2", CultureInfo.CurrentCulture);
            _isPlaying = true;
        }
        else if (_isPlaying)
        {
            _isPlaying = false;
            OnAudioFinished();
        }
    }

    /// <summary>
    /// Resets the audio source clip to null.
    /// </summary>
    public void ResetAudio()
    {
        _audioSource.clip = null; 
    }

    /// <summary>
    /// Callback method called when the audio finishes playing.
    /// </summary>
    void OnAudioFinished()
    {
        pause.gameObject.SetActive(false);
        play.gameObject.SetActive(true);
        _audioSource.time = 0;

    }
}