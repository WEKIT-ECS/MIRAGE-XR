using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Threading.Tasks;
using LearningExperienceEngine.DataModel;
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
    [SerializeField]
    private Slider _progressSlider;

    /// <summary>
    /// The text component used to display the current time of the audio clip being played.
    /// </summary>
    [SerializeField]
    private TMP_Text _currentTimeText;
    [SerializeField]
    private TMP_Text _durationText;

    [SerializeField] private TMP_Text _name;

    /// <summary>
    /// Represents a play button used for audio streaming.
    /// </summary>
    [SerializeField]
    private Button _playButton;
    
    [SerializeField]
    private Button _pauseButton;
    /// <summary>
    /// Reference to the forward button in the UI.
    /// </summary>
    [SerializeField]
    private Button _forward;

    /// <summary>
    /// Reference to the backward button in the UI.
    /// </summary>
    [SerializeField]
    private Button _backward;


    /// <summary>
    /// Represents an AI model for the AiServices.
    /// </summary>
    private AIModel _model;

    /// <summary>
    /// An array of Button objects representing the interactive buttons in the AudioStreamPlayer script. 
    /// </summary>
    private Button[] _buttons;

    /// <summary>
    /// Represents the current playing state of the AudioStreamPlayer.
    /// </summary>
    private bool _isPlaying;
    private bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            _playButton.gameObject.SetActive(!value);
            _pauseButton.gameObject.SetActive(value);
        }
    }

    private const float TimeFactor = 0.1f;
    private const float AudioVolume = 1.0f; 
    


    /// <summary>
    /// Initializes the AudioStreamPlayer by assigning references to its required components.
    /// </summary>
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }


    /// <summary>
    /// Sets up the AudioStreamPlayer component with the specified AIModel.
    /// </summary>
    /// <param name="model">The AIModel to use for setup.</param>
    public async void Setup(AIModel model)
    {
        _audioSource.Stop();
        _model = model;
        _name.text = _model.Name;
        _currentTimeText.text = "0:00";
        _audioSource.clip = await LoadAudioAsync();
        IsPlaying = false;
        _playButton.onClick.AddListener(PlayAudio);
        _pauseButton.onClick.AddListener(PauseAudio);
        _backward.onClick.AddListener(MoveBackward);
        _forward.onClick.AddListener(MoveForward);
        _progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        _durationText.text = _audioSource.clip.length.ToString("F2", CultureInfo.CurrentCulture).Replace(".",":");
        
    }

    /// <summary>
    /// Pauses the audio stream player by stopping the audio source.
    /// </summary>
    private void PauseAudio()
    {
        _audioSource.Stop();
    }

    /// <summary>
    /// Moves the audio playback forward by 10% of the total duration.
    /// </summary>
    private void MoveForward()
    {
        if (_audioSource != null)
        {
            float newTime = _audioSource.time + (_audioSource.clip.length * TimeFactor);
            if (_audioSource.time >= Mathf.Clamp(newTime, 0, _audioSource.clip.length))
            {
                _audioSource.time = Mathf.Clamp(newTime, 0, _audioSource.clip.length);
            }
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
            float newTime = _audioSource.time - (_audioSource.clip.length * TimeFactor);
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
        var message = "Hi I am " + _model.Name;
        AudioClip clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(message, _model.ApiName);
        return clip;
    }


    /// <summary>
    /// Plays the audio clip attached to the audio source. If there is no audio clip attached or the audio source is already playing, this method does nothing.
    /// </summary>
    private void PlayAudio()
    {
        if (_audioSource.clip != null)
        {
            _audioSource.mute = false;
            _audioSource.volume = AudioVolume;
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
            IsPlaying = true;
        }
        else if (IsPlaying)
        {
            IsPlaying = false;
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
        _audioSource.time = 0;
    }
}