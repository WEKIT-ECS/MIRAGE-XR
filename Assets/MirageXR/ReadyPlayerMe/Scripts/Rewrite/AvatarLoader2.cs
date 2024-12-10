using NSubstitute;
using ReadyPlayerMe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MirageXR
{
	public class AvatarLoader2 : MonoBehaviour
	{
		[SerializeField] private string defaultAvatarUrl = "https://models.readyplayer.me/667bed8204fd145bd9e09f19.glb";
		[SerializeField] private AvatarConfig avatarConfig;
		[SerializeField] private GameObject loadingIndicator;

		private AvatarObjectLoader _avatarObjectLoader;

		private GameObject _currentAvatar;

		private AvatarObjectLoader AvatarObjectLoader
		{
			get
			{
				if (_avatarObjectLoader == null)
				{
					_avatarObjectLoader = new AvatarObjectLoader();
					if (avatarConfig != null)
					{
						_avatarObjectLoader.AvatarConfig = avatarConfig;
					}
					else
					{
						Debug.LogWarning("No avatar configuration set. The import of ReadyPlayerMe avatars might not work as expected.", this);
					}
					_avatarObjectLoader.OnCompleted += OnLoadCompleted;
					_avatarObjectLoader.OnFailed += OnLoadFailed;
				}
				return _avatarObjectLoader;
			}
		}

		public string LoadedAvatarUrl { get; private set; }

		public event Action<bool> AvatarLoaded;

		private void Start()
		{
			loadingIndicator.SetActive(false);

			Debug.LogTrace("Loading default avatar");
			LoadAvatar(defaultAvatarUrl);
		}

		public void LoadAvatar(string avatarUrl)
		{
			Debug.LogDebug("Loading avatar " + avatarUrl, this);
			avatarUrl = avatarUrl.Trim();
			loadingIndicator.SetActive(true);
			AvatarObjectLoader.LoadAvatar(avatarUrl);
		}

		private void OnLoadFailed(object sender, FailureEventArgs e)
		{
			loadingIndicator.SetActive(false);
			Debug.LogError("Could not load avatar. Reason: " + e.Message);
			AvatarLoaded?.Invoke(false);
		}

		private void OnLoadCompleted(object sender, CompletionEventArgs e)
		{
			Debug.LogDebug("Loading of avatar successful", this);
			loadingIndicator.SetActive(false);
			if (_currentAvatar != null)
			{
				Destroy(_currentAvatar);
			}
			SetupAvatar(e);
			AvatarLoaded?.Invoke(true);
		}

		// apply the avatar to our player object
		private void SetupAvatar(CompletionEventArgs e)
		{
			LoadedAvatarUrl = e.Url;
			_currentAvatar = e.Avatar;
			// setup transform
			SetupTransform();
			SetupEyes();
			SetupIKRig();
		}

		private void SetupTransform()
		{
			_currentAvatar.transform.parent = transform;
			_currentAvatar.transform.position = Vector3.zero;
			_currentAvatar.transform.rotation = Quaternion.identity;
		}

		private void SetupEyes()
		{
			EyeAnimationHandler eyeAnimation = _currentAvatar.AddComponent<EyeAnimationHandler>();
			eyeAnimation.BlinkInterval = 10f;
		}

		private void SetupIKRig()
		{
			RigBuilder rigBuilder = _currentAvatar.AddComponent<RigBuilder>();

			Transform armature = _currentAvatar.transform.Find("Armature");
			BoneRenderer boneRenderer = armature.gameObject.AddComponent<BoneRenderer>();
			Transform[] bones = armature.GetComponentsInChildren<Transform>();
			boneRenderer.transforms = bones.Where(t => t != armature).ToArray();

			Dictionary<string, Transform> bonesByName = new Dictionary<string, Transform>();
			foreach (Transform t in bones)
			{
				bonesByName.Add(t.name, t);
			}

			GameObject xrIKRig = new GameObject("XR IK Rig");
			xrIKRig.transform.parent = _currentAvatar.transform;
			xrIKRig.transform.position = _currentAvatar.transform.position;
			xrIKRig.transform.rotation = _currentAvatar.transform.rotation;
			Rig rig = xrIKRig.AddComponent<Rig>();
			rigBuilder.layers.Add(new RigLayer(rig));

			SetupIKTargets(rig, bonesByName);

			rigBuilder.Build();
		}

		private void SetupIKTargets(Rig rig, Dictionary<string,Transform> bones)
		{
			AddMuliparentTarget(rig.transform, "Hips", bones);
			AddMuliparentTarget(rig.transform, "Head", bones);
			AddTwoBoneTarget(rig.transform, "LeftArm", "LeftForeArm", "LeftHand", bones);
			AddTwoBoneTarget(rig.transform, "RightArm", "RightForeArm", "RightHand", bones);
			AddTwoBoneTarget(rig.transform, "LeftUpLeg", "LeftLeg", "LeftFoot", bones);
			AddTwoBoneTarget(rig.transform, "RightUpLeg", "RightLeg", "RightFoot", bones);
		}

		private void AddMuliparentTarget(Transform parentRig, string targetedBone, Dictionary<string,Transform> bones)
		{
			GameObject target = new GameObject(targetedBone + "Target");
			target.transform.parent = parentRig;
			target.transform.position = bones[targetedBone].position;
			target.transform.rotation = bones[targetedBone].rotation;

			MultiParentConstraint multiParentConstraint = target.AddComponent<MultiParentConstraint>();
			multiParentConstraint.data.constrainedObject = bones[targetedBone];
			WeightedTransformArray sources = new WeightedTransformArray(0)
			{
				new WeightedTransform(target.transform, 1)
			};
			multiParentConstraint.data.sourceObjects = sources;
			multiParentConstraint.data.constrainedPositionXAxis = true;
			multiParentConstraint.data.constrainedPositionYAxis = true;
			multiParentConstraint.data.constrainedPositionZAxis = true;
			multiParentConstraint.data.constrainedRotationXAxis = true;
			multiParentConstraint.data.constrainedRotationYAxis = true;
			multiParentConstraint.data.constrainedRotationZAxis = true;
		}

		private void AddTwoBoneTarget(Transform parentRig, string rootBone, string midBone, string tipBone, Dictionary<string,Transform> bones)
		{
			GameObject ik = new GameObject(tipBone + "IK");
			ik.transform.parent = parentRig;
			ik.transform.position = bones[tipBone].position;
			ik.transform.rotation = bones[tipBone].rotation;

			GameObject target = new GameObject(tipBone + "IK_target");
			target.transform.parent = ik.transform;
			target.transform.localPosition = Vector3.zero;
			target.transform.localRotation = Quaternion.identity;

			GameObject hint = new GameObject(tipBone + "IK_hint");
			hint.transform.parent = ik.transform;

			TwoBoneIKConstraint ikConstraint = ik.AddComponent<TwoBoneIKConstraint>();
			ikConstraint.data.root = bones[rootBone];
			ikConstraint.data.mid = bones[midBone];
			ikConstraint.data.tip = bones[tipBone];
			ikConstraint.data.target = target.transform;
			ikConstraint.data.hint = hint.transform;

			ikConstraint.data.targetPositionWeight = 1;
			ikConstraint.data.targetRotationWeight = 1;
			ikConstraint.data.hintWeight = 1;
		}
	}
}