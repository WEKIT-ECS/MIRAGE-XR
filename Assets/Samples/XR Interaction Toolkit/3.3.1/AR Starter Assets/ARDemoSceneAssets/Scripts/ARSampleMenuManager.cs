#if AR_FOUNDATION_PRESENT
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets
{
    /// <summary>
    /// Handles dismissing the object menu when clicking out the UI bounds, and showing the
    /// menu again when the create menu button is clicked after dismissal. Manages object deletion in the AR demo scene,
    /// and also handles the toggling between the object creation menu button and the delete button.
    /// </summary>
    public class ARSampleMenuManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Button that opens the create menu.")]
        Button m_CreateButton;

        /// <summary>
        /// Button that opens the create menu.
        /// </summary>
        public Button createButton
        {
            get => m_CreateButton;
            set => m_CreateButton = value;
        }

        [SerializeField]
        [Tooltip("Button that deletes a selected object.")]
        Button m_DeleteButton;

        /// <summary>
        /// Button that deletes a selected object.
        /// </summary>
        public Button deleteButton
        {
            get => m_DeleteButton;
            set => m_DeleteButton = value;
        }

        [SerializeField]
        [Tooltip("The menu with all the creatable objects.")]
        GameObject m_ObjectMenu;

        /// <summary>
        /// The menu with all the creatable objects.
        /// </summary>
        public GameObject objectMenu
        {
            get => m_ObjectMenu;
            set => m_ObjectMenu = value;
        }

        [SerializeField]
        [Tooltip("The animator for the object creation menu.")]
        Animator m_ObjectMenuAnimator;

        /// <summary>
        /// The animator for the object creation menu.
        /// </summary>
        public Animator objectMenuAnimator
        {
            get => m_ObjectMenuAnimator;
            set => m_ObjectMenuAnimator = value;
        }

        [SerializeField]
        [Tooltip("Button that closes the object creation menu.")]
        Button m_CancelButton;

        /// <summary>
        /// Button that closes the object creation menu.
        /// </summary>
        public Button cancelButton
        {
            get => m_CancelButton;
            set => m_CancelButton = value;
        }

        [SerializeField]
        [Tooltip("The interaction group for the AR demo scene.")]
        XRInteractionGroup m_InteractionGroup;

        /// <summary>
        /// The interaction group for the AR demo scene.
        /// </summary>
        public XRInteractionGroup interactionGroup
        {
            get => m_InteractionGroup;
            set => m_InteractionGroup = value;
        }

        [SerializeField]
        XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

        /// <summary>
        /// Input to use for the screen tap start position.
        /// </summary>
        /// <seealso cref="TouchscreenGestureInputController.tapStartPosition"/>
        public XRInputValueReader<Vector2> tapStartPositionInput
        {
            get => m_TapStartPositionInput;
            set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
        }

        /// <summary>
        /// Calls the methods in its invocation list when the spawned object selection changes.
        /// </summary>
        public UnityEvent<int> spawnedObjectSelectionChanged
        {
            get => m_SpawnedObjectSelectionChanged;
            set => m_SpawnedObjectSelectionChanged = value;
        }

        [Header("Events")]
        [SerializeField]
        [Tooltip("Calls the methods in its invocation list when the spawned object selection changes.")]
        UnityEvent<int> m_SpawnedObjectSelectionChanged = new UnityEvent<int>();

        bool m_ShowObjectMenu;

        void OnEnable()
        {
            m_TapStartPositionInput.EnableDirectActionIfModeUsed();
            m_CreateButton.onClick.AddListener(ShowMenu);
            m_CancelButton.onClick.AddListener(HideMenu);
            m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
        }

        void OnDisable()
        {
            m_TapStartPositionInput.DisableDirectActionIfModeUsed();
            m_ShowObjectMenu = false;
            m_CreateButton.onClick.RemoveListener(ShowMenu);
            m_CancelButton.onClick.RemoveListener(HideMenu);
            m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
        }

        void Start()
        {
            HideMenu();
        }

        void Update()
        {
            if (m_ShowObjectMenu)
            {
                m_CreateButton.gameObject.SetActive(false);
                m_DeleteButton.gameObject.SetActive(false);
                var isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
                if (!isPointerOverUI && m_TapStartPositionInput.TryReadValue(out _))
                {
                    HideMenu();
                }
            }
            else if (m_InteractionGroup is not null)
            {
                var currentFocusedObject = m_InteractionGroup.focusInteractable;
                if (currentFocusedObject != null && (!m_DeleteButton.isActiveAndEnabled || m_CreateButton.isActiveAndEnabled))
                {
                    m_CreateButton.gameObject.SetActive(false);
                    m_DeleteButton.gameObject.SetActive(true);
                }
                else if (currentFocusedObject == null && (!m_CreateButton.isActiveAndEnabled || m_DeleteButton.isActiveAndEnabled))
                {
                    m_CreateButton.gameObject.SetActive(true);
                    m_DeleteButton.gameObject.SetActive(false);
                }
            }
        }

        public void SetObjectToSpawn(int objectIndex)
        {
            m_SpawnedObjectSelectionChanged.Invoke(objectIndex);
            HideMenu();
        }

        void ShowMenu()
        {
            m_ShowObjectMenu = true;
            m_ObjectMenu.SetActive(true);
            if (!m_ObjectMenuAnimator.GetBool("Show"))
            {
                m_ObjectMenuAnimator.SetBool("Show", true);
            }
        }

        /// <summary>
        /// Triggers hide animation for menu.
        /// </summary>
        public void HideMenu()
        {
            m_ObjectMenuAnimator.SetBool("Show", false);
            m_ShowObjectMenu = false;
        }

        void DeleteFocusedObject()
        {
            if (m_InteractionGroup == null)
                return;

            var currentFocusedObject = m_InteractionGroup.focusInteractable;
            if (currentFocusedObject != null)
            {
                Destroy(currentFocusedObject.transform.gameObject);
            }
        }
    }
}
#endif
