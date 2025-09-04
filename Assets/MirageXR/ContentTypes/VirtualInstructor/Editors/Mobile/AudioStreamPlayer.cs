using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using JetBrains.Annotations;

/// <summary>
/// Represents an audio stream player for playing audio clips.
/// </summary>
public class AudioStreamPlayer : MonoBehaviour
{
	[Header("UI References")]
	/// <summary>
	/// Represents a progress slider control used for audio streaming.
	/// </summary>
	[SerializeField]
	private Slider _progressSlider;
	/// <summary>
	/// The text component used to display the current time of the audio clip being played.
	/// </summary>
	[SerializeField]
	private TMP_Text _currentTimeTextLabel;
	[SerializeField]
	private TMP_Text _durationTextLabel;

	[SerializeField]
	private GameObject _playImage;

	[SerializeField]
	private GameObject _pauseImage;

	[SerializeField]
	private GameObject _loadingImage;

	[SerializeField] private Button _playPauseButton;

	/// <summary>
	/// Reference to the forward button in the UI.
	/// </summary>
	[SerializeField]
	private Button _forwardButton;

	/// <summary>
	/// Reference to the backward button in the UI.
	/// </summary>
	[SerializeField]
	private Button _backwardButton;

	// The AudioSource used for playing audio streams.
	private AudioSource _audioSource;

	/// <summary>
	/// Represents the current playing state of the AudioStreamPlayer.
	/// </summary>
	public bool IsPlaying
	{
		get; private set;
	}

	private bool _showLoadingState;
	public bool ShowLoadingState
	{
		get => _showLoadingState;
		set
		{
			_showLoadingState = value;
			UpdatePlayPauseButton();
		}
	}

	/// <summary>
	/// The audio clip which is currently loaded into this UI element to play it
	/// </summary>
	public AudioClip AudioClip
	{
		get
		{
			return _audioSource.clip;
		}
		private set
		{
			_audioSource.clip = value;
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
		if (_audioSource == null)
		{
			_audioSource = gameObject.AddComponent<AudioSource>();
		}

		_playPauseButton.onClick.AddListener(TogglePlayPause);
		_backwardButton.onClick.AddListener(SkipBackward);
		_forwardButton.onClick.AddListener(SkipForward);

		_progressSlider.onValueChanged.AddListener(OnSliderValueChanged);

		UpdateTimeLabels();
	}

	private void UpdateTimeLabels()
	{
		if (_audioSource.clip == null)
		{
			_currentTimeTextLabel.text = "0:00";
			_durationTextLabel.text = "0:00";
		}
		else
		{
			_currentTimeTextLabel.text = FormatTime(_audioSource.time);
			_durationTextLabel.text = FormatTime(_audioSource.clip.length);
		}
	}

	private string FormatTime(float timeInSeconds)
	{
		int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
		int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}

	public void SetAudioClip(AudioClip audioClip)
	{
		_audioSource.Stop();
		_audioSource.playOnAwake = false;
		_audioSource.clip = audioClip;
		IsPlaying = false;
		UpdateTimeLabels();
	}

	private void TogglePlayPause()
	{
		if (_audioSource.clip == null)
		{
			return;
		}

		if (IsPlaying)
		{
			PauseAudio();
		}
		else
		{
			PlayAudio();
		}

		UpdatePlayPauseButton();
	}

	/// <summary>
	/// Plays the audio clip attached to the audio source. If there is no audio clip attached or the audio source is already playing, this method does nothing.
	/// </summary>
	public void PlayAudio()
	{
		if (_audioSource.clip != null)
		{
			_audioSource.mute = false;
			_audioSource.volume = AudioVolume;
			_audioSource.Play();
			IsPlaying = true;

			UpdatePlayPauseButton();
		}
	}

	/// <summary>
	/// Pauses the audio stream player by stopping the audio source.
	/// </summary>
	public void PauseAudio()
	{
		_audioSource.Pause();
		IsPlaying = false;
		UpdatePlayPauseButton();
	}


	/// <summary>
	/// Resets the audio source clip to null.
	/// </summary>
	public void ResetAudio()
	{
		_audioSource.Stop();
		IsPlaying = false;
		_audioSource.clip = null;
	}

	/// <summary>
	/// Moves the audio playback forward by 10% of the total duration.
	/// </summary>
	public void SkipForward()
	{
		if (_audioSource.clip == null)
		{
			return;
		}

		int skipAmount = Mathf.RoundToInt(_audioSource.clip.samples * TimeFactor);

		_audioSource.timeSamples = Mathf.Min(_audioSource.clip.samples - 1, _audioSource.timeSamples + skipAmount);
		_progressSlider.value = _audioSource.time / _audioSource.clip.length;
		UpdateTimeLabels();
	}

	/// <summary>
	/// Moves the audio playback position backward by 10% of the total duration.
	/// </summary>
	public void SkipBackward()
	{
		if (_audioSource == null)
		{
			return;
		}

		int skipAmount = Mathf.RoundToInt(_audioSource.clip.samples * TimeFactor);
		_audioSource.timeSamples = Mathf.Max(0, _audioSource.timeSamples - skipAmount);
		_progressSlider.value = _audioSource.time / _audioSource.clip.length;
		UpdateTimeLabels();
	}

	/// <summary>
	/// Called when the value of the Slider component attached to the AudioStreamPlayer changes.
	/// </summary>
	/// <param name="value">The new value of the Slider component.</param>
	private void OnSliderValueChanged(float value)
	{
		if (_audioSource == null || ShowLoadingState)
		{
			return;
		}
		_audioSource.time = value * _audioSource.clip.length;
		UpdateTimeLabels();
	}


	/// <summary>
	/// Updates the audio stream player.
	/// </summary>
	void Update()
	{
		// if we are at the end of the audio clip
		// (internal state is still on playing but audio source stopped playing)
		if (IsPlaying && !ShowLoadingState && _audioSource.clip != null && !_audioSource.isPlaying)
		{
			IsPlaying = false;
			_audioSource.time = 0;
			UpdatePlayPauseButton();
		}

		// keep the progress slider and labels updated
		if (IsPlaying && !ShowLoadingState && _audioSource.clip != null)
		{
			_progressSlider.value = _audioSource.time / _audioSource.clip.length;
			UpdateTimeLabels();
		}
	}

	private void UpdatePlayPauseButton()
	{
		_playPauseButton.enabled = !ShowLoadingState;
		_playImage.SetActive(!IsPlaying && !ShowLoadingState);
		_pauseImage.SetActive(IsPlaying && !ShowLoadingState);
		_loadingImage.SetActive(ShowLoadingState);
	}
}