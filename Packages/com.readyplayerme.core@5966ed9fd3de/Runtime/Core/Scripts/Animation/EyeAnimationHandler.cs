using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class adds a blink animation at regular intervals to an avatar <c>SkeletonMeshRenderer</c> using blendshapes
    /// and bone rotation adjustments.
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("Ready Player Me/Eye Animation Handler", 0)]
    public class EyeAnimationHandler : MonoBehaviour
    {
        private const int VERTICAL_MARGIN = 15;
        private const int HORIZONTAL_MARGIN = 5;
        private const string EYE_BLINK_LEFT_BLEND_SHAPE_NAME = "eyeBlinkLeft";
        private const string EYE_BLINK_RIGHT_BLEND_SHAPE_NAME = "eyeBlinkRight";
        private const float EYE_BLINK_MULTIPLIER = 1f;
        private const float HALFBODY_OFFSET_X = 90;
        private const float HALFBODY_OFFSET_Z = 180;
        private const string MISSING_EYE_BONES_MESSAGE = "Eye bones are required for EyeAnimationHandler.cs but they were not found on loaded Avatar! Eye rotation animations will not be applied";
        private const string MISSING_MORPH_TARGETS_MESSAGE =
            "The 'eyeBlinkLeft' & 'eyeBlinkRight' morph targets are required for EyeAnimationHandler.cs but they were not found on Avatar mesh! Use an AvatarConfig to specify the morph targets to be included on loaded avatars.";
        private const string NO_EYE_BONES_FOUND_IN_AVATAR_SKELETON = "No eye bones found in Avatar skeleton!";

        [SerializeField, Range(0, 1), Tooltip("Effects the duration of the avatar blink animation in seconds.")]
        private float blinkDuration = 0.1f;

        [SerializeField, Range(1, 10), Tooltip("Effects the amount of time in between each blink in seconds.")]
        private float blinkInterval = 3f;

        private WaitForSeconds blinkDelay;
        private Task blinkCoroutine;

        private SkinnedMeshRenderer headMesh;
        private int eyeBlinkLeftBlendShapeIndex = -1;
        private int eyeBlinkRightBlendShapeIndex = -1;

        private Transform leftEyeBone;
        private Transform rightEyeBone;

        private bool isFullBody;
        private bool hasBlinkBlendShapes;
        private bool hasEyeBones;
        private bool CanAnimate => hasBlinkBlendShapes || hasEyeBones;

        private CancellationTokenSource ctxSource;
        public float BlinkDuration
        {
            get => blinkDuration;
            set
            {
                blinkDuration = value;
                if (Application.isPlaying) Initialize();
            }
        }

        public float BlinkInterval
        {
            get => blinkDuration;
            set
            {
                blinkInterval = value;
                if (Application.isPlaying) Initialize();
            }
        }

        /// <summary>
        /// This method is used to setup the coroutine and repeating functions.
        /// </summary>
        public void Initialize()
        {
            blinkDelay = new WaitForSeconds(blinkDuration);

            CancelInvoke();
            InvokeRepeating(nameof(AnimateEyes), 1, blinkInterval);
        }

        /// <summary>
        /// This method is called when the scene is loaded and is used to setup properties and references.
        /// </summary>
        private void Awake()
        {
            UpdateHeadMesh();
            ValidateSkeleton();

            if (!hasBlinkBlendShapes)
            {
                Debug.LogWarning(MISSING_MORPH_TARGETS_MESSAGE);
            }
            else if (!hasEyeBones)
            {
                Debug.LogWarning(MISSING_EYE_BONES_MESSAGE);
            }

            if (!CanAnimate)
            {
                Reset();
                enabled = false;
            }
        }

        public void UpdateHeadMesh()
        {
            headMesh = gameObject.GetMeshRenderer(MeshType.HeadMesh, true);
            hasBlinkBlendShapes = HasBlinkBlendshapes();
        }

        private bool HasBlinkBlendshapes()
        {
            eyeBlinkLeftBlendShapeIndex = headMesh.sharedMesh.GetBlendShapeIndex(EYE_BLINK_LEFT_BLEND_SHAPE_NAME);
            eyeBlinkRightBlendShapeIndex = headMesh.sharedMesh.GetBlendShapeIndex(EYE_BLINK_RIGHT_BLEND_SHAPE_NAME);
            return eyeBlinkLeftBlendShapeIndex > -1 && eyeBlinkRightBlendShapeIndex > -1;
        }

        private void ValidateSkeleton()
        {
            isFullBody = AvatarBoneHelper.IsFullBodySkeleton(transform);
            leftEyeBone = AvatarBoneHelper.GetLeftEyeBone(transform, isFullBody);
            rightEyeBone = AvatarBoneHelper.GetRightEyeBone(transform, isFullBody);
            hasEyeBones = leftEyeBone != null && rightEyeBone != null;
            if (!hasEyeBones)
            {
                Debug.LogWarning(NO_EYE_BONES_FOUND_IN_AVATAR_SKELETON);
            }
        }

        private void Reset()
        {
            CancelInvoke();
            ctxSource?.Cancel();
        }

        private void OnEnable()
        {
            if (CanAnimate)
            {
                Initialize();
            }
        }

        private void OnDisable()
        {
            Reset();
        }

        private void OnDestroy()
        {
            Reset();
        }

        /// <summary>
        /// Rotates the eyes and assigns the blink coroutine. Called in the Initialize method.
        /// </summary>
        private async void AnimateEyes()
        {
            if (hasEyeBones)
            {
                RotateEyes();
            }

            if (hasBlinkBlendShapes)
            {
                ctxSource = new CancellationTokenSource();
                await BlinkEyes(ctxSource.Token);
            }
        }

        /// <summary>
        /// Rotates the eye bones in a random direction.
        /// </summary>
        private void RotateEyes()
        {
            leftEyeBone.localRotation = rightEyeBone.localRotation = GetRandomLookRotation();
        }

        private Quaternion GetRandomLookRotation()
        {
            float vertical = Random.Range(-VERTICAL_MARGIN, VERTICAL_MARGIN);
            float horizontal = Random.Range(-HORIZONTAL_MARGIN, HORIZONTAL_MARGIN);

            Quaternion rotation = isFullBody
                ? Quaternion.Euler(horizontal, vertical, 0)
                : Quaternion.Euler(horizontal - HALFBODY_OFFSET_X, 0, vertical + HALFBODY_OFFSET_Z);
            return rotation;
        }

        /// <summary>
        /// A coroutine that manipulates BlendShapes to open and close the eyes.
        /// </summary>
        /// <param name="ctx"></param>
        private async Task BlinkEyes(CancellationToken ctx)
        {
            headMesh.SetBlendShapeWeight(eyeBlinkLeftBlendShapeIndex, EYE_BLINK_MULTIPLIER);
            headMesh.SetBlendShapeWeight(eyeBlinkRightBlendShapeIndex, EYE_BLINK_MULTIPLIER);

            var startTime = Time.time;
            while (Time.time - startTime < blinkDuration && !ctx.IsCancellationRequested)
            {
                await Task.Yield();
            }

            if (ctx.IsCancellationRequested) return;

            headMesh.SetBlendShapeWeight(eyeBlinkLeftBlendShapeIndex, 0);
            headMesh.SetBlendShapeWeight(eyeBlinkRightBlendShapeIndex, 0);
        }
    }
}
