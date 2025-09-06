using LearningExperienceEngine.DataModel;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
	public class VoiceSettingsMenu : MonoBehaviour
	{
		[SerializeField] private DraggableBottomPopup _audioPlayerDialog;

		private AudioStreamPlayer _audioPlayer;

		private void Start()
		{
			_audioPlayerDialog.gameObject.SetActive(false);
		}

		public async UniTask PlayVoicePreviewAsync(AIModel model, string instructions)
		{
			var message = $"Hi, I am {model.Name}";
			_audioPlayerDialog.Title = model.Name;
			_audioPlayerDialog.OpenDialog();
			if (_audioPlayer == null)
			{
				_audioPlayer = _audioPlayerDialog.DialogContent.GetComponentInChildren<AudioStreamPlayer>();
			}
			_audioPlayer.ShowLoadingState = true;
			var voiceClip = await LoadAudioAsync(model, message, instructions);
			_audioPlayer.ShowLoadingState = false;
			_audioPlayer.SetAudioClip(voiceClip);
			_audioPlayer.PlayAudio();
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
		private async Task<AudioClip> LoadAudioAsync(AIModel model, string message, string instructions)
		{
			var clip = await RootObject.Instance.LEE.ArtificialIntelligenceManager.ConvertTextToSpeechAsync(message, model.ApiName, instructions);
			return clip;
		}
	}
}
