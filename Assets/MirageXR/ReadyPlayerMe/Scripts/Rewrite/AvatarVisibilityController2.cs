using System;
using System.Collections;
using UnityEngine;

namespace MirageXR
{
	public class AvatarVisibilityController2 : AvatarBaseController
	{
		[field: SerializeField] public Material FadeMaterial { get; set; }
		[field: SerializeField] private float FadeDuration { get; set; } = 2.0f;
		
		private SkinnedMeshRenderer[] _avatarRenderers;

		private Material[] _originalMaterials;
		private Material[] _fadeMaterialInstances;

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
					float fadeDuration = FadeVisibility ? FadeDuration : 0f;
					_fadeCoroutine = StartCoroutine(Fade(fadeDuration));
					VisibilityChanged?.Invoke(value);
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			_avatarRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			_originalMaterials = new Material[_avatarRenderers.Length];
			_fadeMaterialInstances = new Material[_avatarRenderers.Length];
			for (int i=0;i<_avatarRenderers.Length;i++)
			{
				_originalMaterials[i] = _avatarRenderers[i].material;
				_fadeMaterialInstances[i] = new Material(FadeMaterial);
			}
			_currentCutoff = FadeMaterialsGetFloat(_cutoffProperty);


			if (!Visible)
			{
				SwitchToFadeableMaterial();
				FadeMaterialsSetFloat(_cutoffProperty, _currentCutoff);
			}
		}

		private float FadeMaterialsGetFloat(string name)
		{
			if (_fadeMaterialInstances.Length > 0)
			{
				return _fadeMaterialInstances[0].GetFloat(name);
			}
			else
			{
				return 0;
			}
		}

		private void FadeMaterialsSetFloat(string name, float value)
		{
			for (int i = 0; i < _fadeMaterialInstances.Length; i++)
			{
				_fadeMaterialInstances[i].SetFloat(name, value);
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
				FadeMaterialsSetFloat(_cutoffProperty, _currentCutoff);
				yield return null;
			}

			_currentCutoff = _targetCutoff;
			FadeMaterialsSetFloat(_cutoffProperty, _currentCutoff);

			if (_visible && _fadeMaterialActive)
			{
				SwitchToOriginalMaterial();
			}
		}

		private void SwitchToFadeableMaterial()
		{
			for (int i = 0; i < _originalMaterials.Length; i++)
			{
				CopyMaterial(_originalMaterials[i], _fadeMaterialInstances[i]);
				_avatarRenderers[i].material = _fadeMaterialInstances[i];
			}
			Debug.Log("Applied fade materials");
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
				for (int i=0;i< _originalMaterials.Length;i++)
				{
					_avatarRenderers[i].material = _originalMaterials[i];
					Debug.Log("Applied original materials");
				}
				_fadeMaterialActive = false;
			}
		}
	}
}
