using System;
using System.Collections;
using UnityEngine;

namespace MirageXR
{
	public class AvatarVisibilityController : MonoBehaviour
	{
		[SerializeField] private Material _fadeMaterial;
		[SerializeField] private float _fadeDuration = 2.0f;
		[SerializeField] private Renderer _avatarRenderer;

		private Material _originalMaterial;
		private Material _fadeMaterialInstance;

		private bool _visible = true;
		private float _targetCutoff;
		private float _currentCutoff;
		private Coroutine _fadeCoroutine;
		private bool _fadeMaterialActive = false;

		private const string _cutoffProperty = "alphaCutoff";

		public bool FadeVisibility { get; set; } = true;

		public event Action<bool> VisibilityChanged;

		public bool Visible
		{
			get => _visible;
			set
			{
				if (_visible != value)
				{
					Debug.Log("Changing avatar visibility to " + value);
					_visible = value;
					_targetCutoff = _visible ? 0f : 1f;
					if (_fadeCoroutine != null)
					{
						StopCoroutine(_fadeCoroutine);
					}
					float fadeDuration = FadeVisibility ? _fadeDuration : 0f;
					_fadeCoroutine = StartCoroutine(Fade(fadeDuration));
					VisibilityChanged?.Invoke(value);
				}
			}
		}

		private AvatarLoader _avatarLoader;
		private AvatarLoader AvatarLoader { get => ComponentUtilities.GetOrFetchComponent(this, ref _avatarLoader); }

		void Awake()
		{
			_fadeMaterialInstance = new Material(_fadeMaterial);
			_currentCutoff = _fadeMaterialInstance.GetFloat(_cutoffProperty);
			AvatarLoader.AvatarLoaded += OnAvatarLoaded;
		}

		private void OnDestroy()
		{
			AvatarLoader.AvatarLoaded -= OnAvatarLoaded;
		}

		private void OnAvatarLoaded(bool successful)
		{
			if (successful)
			{
				if (!Visible)
				{
					SwitchToFadeableMaterial();
					_fadeMaterialInstance.SetFloat(_cutoffProperty, _currentCutoff);
				}
			}
		}

		private IEnumerator Fade(float duration)
		{
			float startCutoff = _currentCutoff;
			float timeElapsed = 0f;

			// if visible is false: we know that we need to fade out with our fading material
			if (!_visible && !_fadeMaterialActive)
			{
				SwitchToFadeableMaterial();
			}

			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				_currentCutoff = Mathf.Lerp(startCutoff, _targetCutoff, timeElapsed / duration);
				_fadeMaterialInstance.SetFloat(_cutoffProperty, _currentCutoff);
				yield return null;
			}

			_currentCutoff = _targetCutoff;
			_fadeMaterialInstance.SetFloat(_cutoffProperty, _currentCutoff);

			if (_visible && _fadeMaterialActive)
			{
				SwitchToOriginalMaterial();
			}
		}

		private void SwitchToFadeableMaterial()
		{
			_originalMaterial = _avatarRenderer.material;
			CopyMaterial(_originalMaterial, _fadeMaterialInstance);
			_avatarRenderer.material = _fadeMaterialInstance;
			Debug.Log("Applied fade material");
			_fadeMaterialActive = true;
		}

		private void CopyMaterial(Material originalMaterial, Material newMaterial)
		{
			string[] textures = new string[] {
				"baseColorTexture",
				"normalTexture",
				"metallicRoughnessTexture",
				"emissiveTexture",
				"occlusionTexture"
			};
			foreach (string texture in textures)
			{
				newMaterial.SetTexture(texture, originalMaterial.GetTexture(texture));
			}
		}

		private void SwitchToOriginalMaterial()
		{
			if (_visible && _fadeMaterialActive)
			{
				_avatarRenderer.material = _originalMaterial;
				Debug.Log("Applied original material");
				_fadeMaterialActive = false;
			}
		}
	}
}
