namespace MirageXR
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class Pick : MonoBehaviour
    {
        [SerializeField] private Transform placeLocation;
        [SerializeField] private GameObject pickOb;
        [SerializeField] private float correctionDistance;
        [SerializeField] private bool resetOnMiss = true;
        [SerializeField] private SpriteToggle lockToggle;
        [SerializeField] private Button changeModelButton;
        [SerializeField] private Text hoverGuide;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip correctAudio;
        [SerializeField] private AudioClip incorrectAudio;

        private bool shouldPlaySound;

        private Vector3 resetPosition;
        private Quaternion resetRotation;
        private bool isMoving = false;
        private bool moveMode = true;
        private float targetRadius;
        private Color originalArrowColor;

        private bool isTrigger = false;
        private int triggerStepIndex;
        private float triggerDuration;

        private bool editMode = false;

        private const string LockHelpText = "When locked the arrow (or 3D model) will bounce back to this location if it is not correctly placed on the target";
        private const string ModelButtonHelpText = "Click this button and select a 3D model from the augmentation list to change the pick and place object model";

        private static MirageXR.ActivityManager ActivityManager => MirageXR.RootObject.Instance.activityManager;

        public bool EditMode
        {
            get { return editMode; }
            set { editMode = value; }
        }

        public Vector3 ResetPosition
        {
            get { return resetPosition; }
            set { resetPosition = value; }
        }

        public Quaternion ResetRotation
        {
            get { return resetRotation; }
            set { resetRotation = value; }
        }

        public MeshRenderer ArrowRenderer
        {
            get { return GetComponentInChildren<MeshRenderer>(); }
        }

        public Button ChangeModelButton
        {
            get
            {
                return changeModelButton;
            }
        }

        public bool MoveMode
        {
            get => moveMode;
            set => moveMode = value;
        }

        public bool IsTrigger
        {
            get => IsTrigger;
            set => IsTrigger = value;
        }

        public int TriggerStepIndex
        {
            get => TriggerStepIndex;
            set => TriggerStepIndex = value;
        }

        public string MyModelID
        {
            get; set;
        }

        void Start()
        {
            targetRadius = placeLocation.transform.localScale.x / 2;
            pickOb = this.gameObject;
            ChangeCorrectionDistance(targetRadius);
            moveMode = false;
            lockToggle.IsSelected = true;

            originalArrowColor = ArrowRenderer.material.color;

            changeModelButton.onClick.AddListener(CapturePickModel);

            AddHoverGuide(lockToggle.gameObject, LockHelpText);
            AddHoverGuide(changeModelButton.gameObject, ModelButtonHelpText);
            shouldPlaySound = false;
        }

        void Update()
        {
            float targetRadiusUpdate = placeLocation.transform.localScale.x / 2;

            if (!moveMode)
            {
                if (transform.hasChanged)
                {
                    ManipulationStart();
                }
                else if (!transform.hasChanged)
                {
                    ManipulationStop();
                }
                transform.hasChanged = false;
            }
            else if (moveMode && ArrowRenderer.material.color != originalArrowColor)
            {
                ArrowRenderer.material.SetColor("_Color", originalArrowColor);
            }

            if (targetRadius != targetRadiusUpdate)
            {
                targetRadius = targetRadiusUpdate;

                ChangeCorrectionDistance(targetRadius);
            }
        }

        /// <summary>
        /// Ready to select a model augmentation for replcaing with the arrow
        /// </summary>
        private void CapturePickModel()
        {
            changeModelButton.GetComponent<Image>().color = Color.red;
            ActionEditor.Instance.pickArrowModelCapturing = (true, this);
        }

        public void SetMoveMode()
        {
            if (moveMode)
            {
                moveMode = false;
                ResetPosition = pickOb.transform.localPosition;
                ResetRotation = pickOb.transform.localRotation;
                shouldPlaySound = false;
            }
            else
            {
                moveMode = true;
            }

            lockToggle.ToggleValue();
        }

        public void SetRestOnMiss(bool reset)
        {
            resetOnMiss = reset;
        }

        /// <summary>
        /// Sets the target transform for the pick object
        /// </summary>
        public void SetTargettransform(Transform target)
        {
            placeLocation = target;
        }

        /// <summary>
        /// Change the distance that will be considered close enough to the desired place location
        /// </summary>
        public void ChangeCorrectionDistance(float distance)
        {
            correctionDistance = distance;
        }

        /// <summary>
        /// Sets isMoving to true to show that the object is being manipulated.
        /// </summary>
        public void ManipulationStart()
        {
            ArrowRenderer.material.SetColor("_Color", originalArrowColor);
            isMoving = true;
        }

        /// <summary>
        /// Checks if the pick object has been placed correctly and sets isMoving to false to show that the object has stopped being manipulated.
        /// </summary>
        public void ManipulationStop()
        {
            if (isMoving)
            {
                if (Mathf.Abs(pickOb.transform.localPosition.x - placeLocation.localPosition.x) <= correctionDistance &&
                Mathf.Abs(pickOb.transform.localPosition.y - placeLocation.localPosition.y) <= correctionDistance &&
                Mathf.Abs(pickOb.transform.localPosition.z - placeLocation.localPosition.z) <= correctionDistance)
                {
                    pickOb.transform.localPosition = new Vector3(placeLocation.localPosition.x, placeLocation.localPosition.y, placeLocation.localPosition.z);

                    ArrowRenderer.material.SetColor("_Color", Color.green);

                    if (shouldPlaySound)
                    {
                        playAudio(correctAudio);
                    }
                    if (isTrigger && !EditMode)
                    {
                        StartCoroutine(triggerAction());
                    }
                }
                else if (resetOnMiss)
                {
                    pickOb.transform.localPosition = ResetPosition;
                    pickOb.transform.localRotation = ResetRotation;

                    ArrowRenderer.material.SetColor("_Color", originalArrowColor);
                    if (shouldPlaySound)
                    {
                        playAudio(incorrectAudio);
                    }
                }
            }
            isMoving = false;
            shouldPlaySound = true;
        }


        private void playAudio(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        private void AddHoverGuide(GameObject obj, string hoverMessage)
        {
            var HoverGuilde = obj.AddComponent<HoverGuilde>();
            HoverGuilde.SetGuildText(hoverGuide);
            HoverGuilde.SetMessage(hoverMessage);
        }

        private IEnumerator triggerAction()
        {
            yield return new WaitForSeconds(triggerDuration);

            ActivityManager.ActivateActionByIndex(triggerStepIndex);
        }

        public void setTrigger(Trigger trigger)
        {
            isTrigger = trigger != null ? true : false;

            if (isTrigger)
            {
                var stepIndex = int.Parse(trigger.value) - 1;

                if (stepIndex > ActivityManager.ActionsOfTypeAction.Count)
                {
                    stepIndex = ActivityManager.ActionsOfTypeAction.Count - 1;
                }

                triggerStepIndex = stepIndex;
                triggerDuration = trigger.duration;
            }
        }
    }
}
