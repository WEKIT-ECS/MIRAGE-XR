using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR.View
{
    public class LabelContentView : ContentView
    {
        private const float MinScale = 0.01f;
        private const float MaxScale = 0.1f;
        private const float MinDistance = 0.1f;
        private const float MaxDistance = 2f;

        [SerializeField] private Transform point;
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button buttonLock;
        [SerializeField] private Image imageBackground;
        [SerializeField] private Image imageIcon;
        [SerializeField] private BoxCollider colliderText;
        [SerializeField] private SphereCollider colliderPoint;
        [SerializeField] private Sprite spriteLock;
        [SerializeField] private Sprite spriteUnlock;
        [SerializeField] private LineRenderer lineRenderer;

        private XRGrabInteractable _xrGrabInteractableBase;
        private XRGrabInteractable _xrGrabInteractableText;
        private XRSimpleInteractable _xrGrabInteractableTextSimple;
        private XRGeneralGrabTransformer _xrGeneralGrabTransformerText;
        private Camera _camera;
        private bool _isBillboarded;
        private bool _isDynamicScale = true;
        private bool _isUpdateLine = true;
        private bool _isCanvasTransformLocked = true;

        protected override async UniTask InitializeContentAsync(Content content)
        {
            await base.InitializeContentAsync(content);

            _camera = RootObject.Instance.BaseCamera;
            if (content is Content<LabelContentData> imageContent)
            {
                Initialized = await InitializeContentAsync(imageContent);
            }
            else
            {
                AppLog.LogError("content is not a Content<ImageContentData>");
            }
        }

        protected override async UniTask OnContentUpdatedAsync(Content content)
        {
            if (content is not Content<LabelContentData> newContent || Content is not Content<LabelContentData> oldContent)
            {
                return;
            }

            Initialized = false;
            Initialized = await InitializeContentAsync(newContent);
            InitializeBillboard(newContent);

            await base.OnContentUpdatedAsync(content);
        }

        private async UniTask<bool> InitializeContentAsync(Content<LabelContentData> content)
        {
            var result = await InitializeTextAsync(content);
            InitializeBillboard(content);
            return result;
        }

        private async UniTask<bool> InitializeTextAsync(Content<LabelContentData> content)
        {
            imageBackground.color = content.ContentData.BackgroundColor;
            text.text = content.ContentData.Text;
            text.color = content.ContentData.FontColor;
            text.fontSize = content.ContentData.FontSize;
            canvas.worldCamera = _camera;
            buttonLock.onClick.AddListener(OnButtonLockClicked);
            LayoutRebuilder.MarkLayoutForRebuild(text.rectTransform);
            await UniTask.NextFrame(PlayerLoopTiming.EarlyUpdate);
            var canvasSize = ((RectTransform)canvas.transform).rect.size;
            colliderText.size = new Vector3(canvasSize.x, canvasSize.y, 2);
            colliderText.center = new Vector3(canvasSize.x * -0.5f, 0, 0);

            return true;
        }

        private void OnButtonLockClicked()
        {
            _isCanvasTransformLocked = !_isCanvasTransformLocked;
            UpdateCanvasLockState();
        }

        private void UpdateCanvasLockState()
        {
            imageIcon.sprite = _isCanvasTransformLocked ? spriteLock : spriteUnlock;
            _xrGrabInteractableBase.colliders.Clear();
            _xrGrabInteractableBase.colliders.Add(colliderPoint);
            if (_isCanvasTransformLocked)
            {
                _xrGrabInteractableBase.colliders.Add(colliderText);
            }

            _xrGrabInteractableText.enabled = !_isCanvasTransformLocked;
            _xrGeneralGrabTransformerText.enabled = !_isCanvasTransformLocked;
        }

        protected override void InitializeManipulator()
        {
            var rigidBody = canvas.gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            _xrGeneralGrabTransformerText = canvas.gameObject.AddComponent<XRGeneralGrabTransformer>();
            _xrGeneralGrabTransformerText.allowTwoHandedScaling = false;

           _xrGrabInteractableText = canvas.gameObject.AddComponent<XRGrabInteractable>();
           _xrGrabInteractableText.movementType = XRBaseInteractable.MovementType.Instantaneous;
           _xrGrabInteractableText.retainTransformParent = true;
           _xrGrabInteractableText.trackScale = false;
           _xrGrabInteractableText.useDynamicAttach = true;
           _xrGrabInteractableText.matchAttachPosition = true;
           _xrGrabInteractableText.matchAttachRotation = true;
           _xrGrabInteractableText.snapToColliderVolume = false;
           _xrGrabInteractableText.reinitializeDynamicAttachEverySingleGrab = false;
           _xrGrabInteractableText.selectMode = InteractableSelectMode.Single;

           base.InitializeManipulator();
            _xrGrabInteractableBase = gameObject.GetComponent<XRGrabInteractable>();
            _xrGrabInteractableBase.selectMode = InteractableSelectMode.Single;
            _xrGrabInteractableBase.trackScale = false;
            UpdateCanvasLockState();
        }

        protected override void InitializeBoxCollider()
        {
        }

        private void InitializeBillboard(Content<LabelContentData> content)
        {
            _isBillboarded = content.ContentData.IsBillboarded;
        }

        private void LateUpdate()
        {
            if (!Initialized)
            {
                return;
            }
            
            if (_isBillboarded)
            {
                DoBillboarding();
            }

            if (_isDynamicScale)
            {
                DoDynamicScale();
            }

            if (_isUpdateLine)
            {
                UpdateLine();
            }
        }

        private void UpdateLine()
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, point.position);
            lineRenderer.SetPosition(1, canvas.transform.position);
        }

        private void DoDynamicScale()
        {
            var distance = Vector3.Distance(_camera.transform.position, point.position);
            if (distance < MinDistance)
            {
                point.localScale = new Vector3(MinScale, MinScale, MinScale);
            }
            if (distance > MaxDistance)
            {
                point.localScale = new Vector3(MaxScale, MaxScale, MaxScale);
            }
            else
            {
                var scale = Mathf.Lerp(MinScale, MaxScale, distance / MaxDistance);
                point.localScale = new Vector3(scale, scale, scale);                
            }
        }

        private void DoBillboarding()
        {
            canvas.transform.rotation =  _camera.transform.rotation;
            
            /*var newRotation = _camera.transform.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            canvas.transform.eulerAngles = newRotation;*/
            
            //canvas.transform.LookAt(_camera.transform);
            //canvas.transform.Rotate(0, 180, 0);
        }
    }
}