using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class NewActivityScreenSpatialView : ScreenView
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonClose;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonCollaborativeSession;
        [SerializeField] private Button _buttonAddNewStep;
        [SerializeField] private Button _buttonThumbnail;
        [SerializeField] private Button _buttonDeleteThumbnail;
        [SerializeField] private Button _nextStep;
        [SerializeField] private Button _previousStep;

        [Header("Images")]
        [SerializeField] private RawImage _imageThumbnail;
        [SerializeField] private Image _imageThumbnailTemp;
        [Header("Text")]
        [SerializeField] private TMP_Text textTitle;
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField inputFieldActivityName;
        [SerializeField] private TMP_InputField inputFieldActivityTags;
        [SerializeField] private TMP_InputField inputFieldActivityDescription;
        [SerializeField] private TMP_Dropdown dropdownVisibility;
        [SerializeField] private TMP_Dropdown dropdownPublicationStatus;
        [SerializeField] private TMP_InputField inputFieldCustomerScope;
        [Header("Toggles")]
        [SerializeField] private Toggle _toggleEditorMode;
        [Header("Action Buttons")]
        [SerializeField] private Button _buttonDuplicate;
        [SerializeField] private Button _buttonStartActivity;
        [Header("Grants")]
        [SerializeField] private Transform _grantsContainer;
        [SerializeField] private GameObject _panelAddGrant;
        [SerializeField] private TMP_Dropdown _dropdownGrantType;
        [SerializeField] private TMP_InputField _inputFieldGranteeId;
        [SerializeField] private TMP_Dropdown _dropdownGrantPrivileges;
        [SerializeField] private Button _buttonAddGrant;
        [Header("Containers")]
        [SerializeField] private RectTransform _thumbnaiContainer;
        [SerializeField] private Transform _stepsContainer;
        [Header("Prefabs")]
        [SerializeField] private StepItemView _stepsItemPrefab;
        [SerializeField] private ImageEditorSpatialView _imageEditorPrefab;
        [Space]
        [Header("Calibration tab")]
        [Header("Buttons and Toggles")]
        [SerializeField] private Button _buttonWireframeVignette;
        [SerializeField] private Button _buttonAssignRoomModel;
        [SerializeField] private Toggle _toggleShowRoomScan;
        [SerializeField] private Button _buttonReposition;
        [Header("Game Objects")]
        [SerializeField] private GameObject _panelCalibrated;
        [SerializeField] private GameObject _panelNotCalibrated;
        [SerializeField] private GameObject _panelRoomScanAdded;
        [SerializeField] private GameObject _panelRoomScanNotAdded;
        [SerializeField] private GameObject _panelHeightNotCalibrated;
        [SerializeField] private GameObject _panelAddNewStep;

        private void Awake()
        {
            if (_buttonDuplicate == null)
            {
                var dupTransform = transform.Find("InfoPanel/DuplicateButton") ?? transform.Find("DuplicateButton");
                if (dupTransform != null) _buttonDuplicate = dupTransform.GetComponent<Button>();
            }

            if (_buttonStartActivity == null)
            {
                var startTransform = transform.Find("InfoPanel/StartButton") ?? transform.Find("StartButton");
                if (startTransform != null) _buttonStartActivity = startTransform.GetComponent<Button>();
            }

            if (inputFieldActivityTags == null)
            {
                var tagsTransform = transform.Find("InfoPanel/TagsInputField") ?? transform.Find("TagsInputField");
                if (tagsTransform != null) inputFieldActivityTags = tagsTransform.GetComponent<TMP_InputField>();
            }

            if (dropdownVisibility == null)
            {
                var visTransform = transform.Find("InfoPanel/VisibilityDropdown") ?? transform.Find("VisibilityDropdown");
                if (visTransform != null) dropdownVisibility = visTransform.GetComponent<TMP_Dropdown>();
            }

            if (dropdownPublicationStatus == null)
            {
                var pubTransform = transform.Find("InfoPanel/PublicationStatusDropdown") ?? transform.Find("PublicationStatusDropdown");
                if (pubTransform != null) dropdownPublicationStatus = pubTransform.GetComponent<TMP_Dropdown>();
            }

            if (inputFieldCustomerScope == null)
            {
                var scopeTransform = transform.Find("InfoPanel/CustomerScopeInputField") ?? transform.Find("CustomerScopeInputField");
                if (scopeTransform != null) inputFieldCustomerScope = scopeTransform.GetComponent<TMP_InputField>();
            }

            if (_grantsContainer == null)
            {
                _grantsContainer = transform.Find("InfoPanel/GrantsContainer") ?? transform.Find("GrantsContainer");
            }
        }

        public void SetActionOnInputFieldActivityNameEditEnd(UnityAction<string> action) => inputFieldActivityName.onEndEdit.AddListener(action);
        public void SetActionOnInputFieldActivityTagsEditEnd(UnityAction<string> action) => inputFieldActivityTags?.onEndEdit.AddListener(action);
        public void SetActionOnInputFieldActivityDescriptionEditEnd(UnityAction<string> action) => inputFieldActivityDescription.onEndEdit.AddListener(action);
        public void SetInputFieldActivityNameText(string text) => inputFieldActivityName.text = text;
        public void SetInputFieldActivityTagsText(string text)
        {
            if (inputFieldActivityTags != null) inputFieldActivityTags.text = text;
        }
        public void SetInputFieldActivityDescriptionText(string text) => inputFieldActivityDescription.text = text;
        public void SetInputFieldActivityNameTextInteractable(bool value) => inputFieldActivityName.interactable = value;
        public void SetInputFieldActivityTagsInteractable(bool value)
        {
            if (inputFieldActivityTags != null) inputFieldActivityTags.interactable = value;
        }
        public void SetInputFieldActivityDescriptionInteractable(bool value) => inputFieldActivityDescription.interactable = value;

        public void SetActionOnDropdownVisibilityChanged(UnityAction<int> action) => dropdownVisibility?.onValueChanged.AddListener(action);
        public void SetDropdownVisibilityValue(int value)
        {
            if (dropdownVisibility != null) dropdownVisibility.value = value;
        }
        public void SetDropdownVisibilityInteractable(bool value)
        {
            if (dropdownVisibility != null) dropdownVisibility.interactable = value;
        }
        public void SetDropdownVisibilityActive(bool value)
        {
            if (dropdownVisibility != null) dropdownVisibility.gameObject.SetActive(value);
        }

        public void SetActionOnDropdownPublicationStatusChanged(UnityAction<int> action) => dropdownPublicationStatus?.onValueChanged.AddListener(action);
        public void SetDropdownPublicationStatusValue(int value)
        {
            if (dropdownPublicationStatus != null) dropdownPublicationStatus.value = value;
        }
        public void SetDropdownPublicationStatusInteractable(bool value)
        {
            if (dropdownPublicationStatus != null) dropdownPublicationStatus.interactable = value;
        }
        public void SetDropdownPublicationStatusActive(bool value)
        {
            if (dropdownPublicationStatus != null) dropdownPublicationStatus.gameObject.SetActive(value);
        }

        public void SetActionOnInputFieldCustomerScopeEditEnd(UnityAction<string> action) => inputFieldCustomerScope?.onEndEdit.AddListener(action);
        public void SetInputFieldCustomerScopeText(string text)
        {
            if (inputFieldCustomerScope != null) inputFieldCustomerScope.text = text;
        }
        public void SetInputFieldCustomerScopeInteractable(bool value)
        {
            if (inputFieldCustomerScope != null) inputFieldCustomerScope.interactable = value;
        }
        public void SetInputFieldCustomerScopeActive(bool value)
        {
            if (inputFieldCustomerScope != null) inputFieldCustomerScope.gameObject.SetActive(value);
        }

        public void SetActionOnButtonAddGrantClick(UnityAction action) => _buttonAddGrant?.SafeSetListener(action);
        public void SetAddGrantPanelActive(bool value)
        {
            if (_panelAddGrant != null) _panelAddGrant.SetActive(value);
        }

        public bool TryGetNewGrantData(out LearningExperienceEngine.DataModel.Grant grant)
        {
            grant = null;
            if (_inputFieldGranteeId == null || string.IsNullOrWhiteSpace(_inputFieldGranteeId.text))
            {
                return false;
            }

            string type = _dropdownGrantType != null && _dropdownGrantType.value == 1 ? "group" : "user";
            string id = _inputFieldGranteeId.text.Trim();
            var privs = new System.Collections.Generic.List<string> { "read" };
            if (_dropdownGrantPrivileges != null && _dropdownGrantPrivileges.value == 1)
            {
                privs.Add("update");
            }

            grant = new LearningExperienceEngine.DataModel.Grant
            {
                GranteeType = type,
                GranteeId = id,
                Privileges = privs
            };
            return true;
        }

        public void ClearNewGrantInputs()
        {
            if (_inputFieldGranteeId != null) _inputFieldGranteeId.text = string.Empty;
        }

        public void SetGrantsList(System.Collections.Generic.List<LearningExperienceEngine.DataModel.Grant> grants, bool isEditor, bool canShare, UnityAction<int> onRemoveGrant)
        {
            if (_grantsContainer == null) return;

            foreach (Transform child in _grantsContainer)
            {
                Destroy(child.gameObject);
            }

            if (grants == null || grants.Count == 0)
            {
                var emptyObj = new GameObject("EmptyGrantsNotice", typeof(RectTransform));
                emptyObj.transform.SetParent(_grantsContainer, false);
                var text = emptyObj.AddComponent<TextMeshProUGUI>();
                text.text = "No additional grants";
                text.fontSize = 14;
                text.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                return;
            }

            for (int i = 0; i < grants.Count; i++)
            {
                int index = i;
                var grant = grants[i];
                var grantItemObj = new GameObject($"GrantItem_{i}", typeof(RectTransform));
                grantItemObj.transform.SetParent(_grantsContainer, false);

                var hLayout = grantItemObj.AddComponent<HorizontalLayoutGroup>();
                hLayout.childControlWidth = false;
                hLayout.childForceExpandWidth = false;
                hLayout.childControlHeight = true;
                hLayout.childForceExpandHeight = false;
                hLayout.spacing = 8f;

                var textObj = new GameObject("GrantText", typeof(RectTransform));
                textObj.transform.SetParent(grantItemObj.transform, false);
                var text = textObj.AddComponent<TextMeshProUGUI>();
                string typeLabel = grant.GranteeType?.ToLower() == "group" ? "[GROUP]" : "[USER]";
                string privsLabel = grant.Privileges != null ? string.Join(", ", grant.Privileges) : "read";
                text.text = $"{typeLabel} {grant.GranteeId} ({privsLabel})";
                text.fontSize = 14;

                if (isEditor && canShare)
                {
                    var removeBtnObj = new GameObject("RemoveBtn", typeof(RectTransform));
                    removeBtnObj.transform.SetParent(grantItemObj.transform, false);
                    var removeBtn = removeBtnObj.AddComponent<Button>();
                    var removeBtnText = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
                    removeBtnText.transform.SetParent(removeBtnObj.transform, false);
                    removeBtnText.text = "✕";
                    removeBtnText.fontSize = 14;
                    removeBtnText.color = Color.red;
                    removeBtn.onClick.AddListener(() => onRemoveGrant?.Invoke(index));
                }
            }
        }

        public void SetActionOnButtonDuplicateClick(UnityAction action) => _buttonDuplicate?.SafeSetListener(action);
        public void SetButtonDuplicateActive(bool value)
        {
            if (_buttonDuplicate != null) _buttonDuplicate.gameObject.SetActive(value);
        }

        public void SetActionOnButtonStartActivityClick(UnityAction action) => _buttonStartActivity?.SafeSetListener(action);
        public void SetButtonStartActivityActive(bool value)
        {
            if (_buttonStartActivity != null) _buttonStartActivity.gameObject.SetActive(value);
        }
        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonCloseClick(UnityAction action) => _buttonClose.SafeSetListener(action);
        public void SetActionOnButtonSettingsClick(UnityAction action) => _buttonSettings.SafeSetListener(action);
        public void SetActionOnButtonCollaborativeSessionClick(UnityAction action) => _buttonCollaborativeSession.SafeSetListener(action);
        public void SetActionOnButtonAddNewStepClick(UnityAction action) => _buttonAddNewStep.SafeSetListener(action);
        public void SetActionOnButtonThumbnailClick(UnityAction action) => _buttonThumbnail.SafeSetListener(action);
        public void SetActionOnButtonDeleteThumbnailClick(UnityAction action) => _buttonDeleteThumbnail?.SafeSetListener(action);
        public void SetButtonDeleteThumbnailActive(bool active)
        {
            if (_buttonDeleteThumbnail != null)
            {
                _buttonDeleteThumbnail.gameObject.SetActive(active);
                if (active)
                {
                    _buttonDeleteThumbnail.transform.SetAsLastSibling();
                }
            }
        }
        public void SetActionOnButtonNextStepClick(UnityAction action) => _nextStep.SafeSetListener(action);
        public void SetActionOnButtonPreviousStepClick(UnityAction action) => _previousStep.SafeSetListener(action);
        public void RemoveActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeRemoveListener(action);
        public void SetActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeAddListener(action);
        public void SetIsToggleEditorOn(bool value) => _toggleEditorMode.SafeSetIsOn(value);
        public void SetPanelAddNewStepActive(bool value) => _panelAddNewStep.SetActive(value);
        public Vector2 GetImageThumbnailContainerSize() => _thumbnaiContainer.rect.size;
        public void SetImageThumbnailActive(bool value)
        {
            if (_imageThumbnail != null)
            {
                _imageThumbnail.gameObject.SetActive(false);
            }
            if (_imageThumbnailTemp != null)
            {
                _imageThumbnailTemp.gameObject.SetActive(value);
            }
        }
        //public void SetImageThumbnailTexture(Texture2D texture2D) => _imageThumbnailTemp.texture = texture2D;
        public void SetImageThumbnailTexture(Sprite sprite) => _imageThumbnailTemp.sprite = sprite;
        public void SetTitleText(string text) => textTitle.text = text;
        public void SetImageThumbnailSize(Vector2 size)
        {
            if (_imageThumbnail != null)
            {
                _imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                _imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
            if (_imageThumbnailTemp != null)
            {
                _imageThumbnailTemp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                _imageThumbnailTemp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
        }

        public Transform GetStepsContainer()
        {
            return _stepsContainer;
        }

        public StepItemView GetStepsItemPrefab()
        {
            return _stepsItemPrefab;
        }
        
        private TabSwitcher _tabSwitcher;
        public TabSwitcher TabSwitcher => _tabSwitcher != null ? _tabSwitcher : (_tabSwitcher = GetComponentInChildren<TabSwitcher>(true));

        public void SwitchToStepsTab()
        {
            TabSwitcher?.SwitchTab(0);
        }

        public void SwitchToInfoTab()
        {
            TabSwitcher?.SwitchTab(1);
        }

        //#region Calibration tab
        public void SetActionOnButtonWireframeVignetteClick(UnityAction action) => _buttonWireframeVignette.SafeSetListener(action);
        public void SetActionOnButtonAssignRoomModelClick(UnityAction action) => _buttonAssignRoomModel.SafeSetListener(action);
        public void SetActionOnToggleShowRoomScanValueChanged(UnityAction<bool> action) => _toggleShowRoomScan.SafeSetListener(action);
        public void SetIsToggleShowRoomScanOnWithoutNotify(bool value) => _toggleShowRoomScan.SafeSetIsOn(value);
        public void SetActionOnButtonRepositionClick(UnityAction action) => _buttonReposition.SafeSetListener(action);

        public void SetAssignRoomModelInteractable(bool value)
        {
            if (_buttonAssignRoomModel != null)
            {
                _buttonAssignRoomModel.interactable = value;
            }
        }

        public void SetRepositionInteractable(bool value)
        {
            if (_buttonReposition != null)
            {
                _buttonReposition.interactable = value;
            }
        }
    }
}
