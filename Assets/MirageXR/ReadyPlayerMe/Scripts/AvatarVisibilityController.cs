using System.Collections;
using System.Collections.Generic;
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

		private const string _cutoffProperty = "alphaCutoff";

		public bool Visible
		{
			get => _visible;
			set
			{
				if (_visible != value)
				{
					_visible = value;
					_targetCutoff = _visible ? 0f : 1f;
					if (_fadeCoroutine != null)
					{
						StopCoroutine(_fadeCoroutine);
					}
					_fadeCoroutine = StartCoroutine(Fade());
				}
			}
		}

		void Start()
		{
			_fadeMaterialInstance = new Material(_fadeMaterial);
			_currentCutoff = _fadeMaterialInstance.GetFloat(_cutoffProperty);
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

		private IEnumerator Fade()
		{
			float startCutoff = _currentCutoff;
			float timeElapsed = 0f;

			// if visible is false: we know that we need to fade out with our fading material
			if (!_visible && _avatarRenderer.material != _fadeMaterialInstance)
			{
				_originalMaterial = _avatarRenderer.material;
				CopyMaterial(_originalMaterial, _fadeMaterialInstance);
				_avatarRenderer.material = _fadeMaterialInstance;
			}

			while (timeElapsed < _fadeDuration)
			{
				timeElapsed += Time.deltaTime;
				_currentCutoff = Mathf.Lerp(startCutoff, _targetCutoff, timeElapsed / _fadeDuration);
				_fadeMaterialInstance.SetFloat(_cutoffProperty, _currentCutoff);
				yield return null;
			}

			_currentCutoff = _targetCutoff;
			_fadeMaterialInstance.SetFloat(_cutoffProperty, _currentCutoff);

			// if visible is true: we can replace the fading material with our original material
			if (_visible && _avatarRenderer.material == _fadeMaterialInstance)
			{
				_avatarRenderer.material = _originalMaterial;
			}
		}
	}
}
