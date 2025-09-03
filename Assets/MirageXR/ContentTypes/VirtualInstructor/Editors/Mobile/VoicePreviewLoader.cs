using LearningExperienceEngine.DataModel;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class VoicePreviewLoader : MonoBehaviour
	{
		[SerializeField] private DraggableBottomPopup _audioPlayerDialog;

		private AudioStreamPlayer _audioPlayer;

		private void Start()
		{
			_audioPlayerDialog.gameObject.SetActive(false);
		}

		public async void PlayVoicePreview(AIModel model)
		{
			string message = "Hi I am " + model.Name;
			_audioPlayerDialog.Title = message;
			_audioPlayerDialog.OpenDialog();
			if (_audioPlayer == null)
			{
				_audioPlayer = _audioPlayerDialog.Content.GetComponentInChildren<AudioStreamPlayer>();
			}
			_audioPlayer.ShowLoadingState = true;
			AudioClip voiceClip = await LoadAudioAsync(model, message);
			_audioPlayer.ShowLoadingState = false;
			_audioPlayer.SetAudioClip(voiceClip);
		}

		/// <summary>
		/// Loads an audio clip asynchronously.
		/// </summary>
		/// <returns>The loaded audio clip.</returns>
		/// <remarks>
		/// This method is used to load an audio clip asynchronously. It uses the given text message
		/// based on the AI model's name and passes it to the AIManager's ConvertTextToSpeechAsync method
		/// along with the model's API name. The method waits for the audio clip to be retrieved and returns it.
		/// </remarks>
		private async Task<AudioClip> LoadAudioAsync(AIModel model, string message)
		{
			AudioClip clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(message, model.ApiName);
			return clip;
		}
	}
}
