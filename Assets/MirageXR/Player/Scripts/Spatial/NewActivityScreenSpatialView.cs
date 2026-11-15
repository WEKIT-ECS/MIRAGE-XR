using System.Collections;
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
        [Header("License & Creator")]
        [SerializeField] private TMP_Text textCreatorAttribution;
        [SerializeField] private Toggle toggleLicenseBY;
        [SerializeField] private Toggle toggleLicenseSA;
        [SerializeField] private Toggle toggleLicenseND;
        [SerializeField] private Toggle toggleLicenseNC;
        [Header("Containers")]
        [SerializeField] private RectTransform _thumbnaiContainer;
        [SerializeField] private Transform _stepsContainer;
        [Header("Scroll")]
        [SerializeField] private ScrollRect _infoScrollRect;

        public ScrollRect InfoScrollRect
        {
            get
            {
                if (_infoScrollRect == null)
                {
                    var scrolls = GetComponentsInChildren<ScrollRect>(true);
                    foreach (var s in scrolls)
                    {
                        if (s.name == "InfoScrollView")
                        {
                            _infoScrollRect = s;
                            break;
                        }
                    }
                }
                return _infoScrollRect;
            }
        }
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

        protected override void Awake()
        {
            base.Awake();

            var blurredBg = transform.Find("Blurred Background");
            if (blurredBg != null)
            {
                blurredBg.gameObject.SetActive(false);
            }

            InitializeDropdowns();

            if (inputFieldActivityDescription != null)
            {
                inputFieldActivityDescription.lineType = TMP_InputField.LineType.MultiLineNewline;
            }

            if (_buttonThumbnail == null)
            {
                var btns = GetComponentsInChildren<Button>(true);
                foreach (var b in btns)
                {
                    if (b.name == "AddPreviewButton_Spatial")
                    {
                        _buttonThumbnail = b;
                        break;
                    }
                }
            }

            if (_imageThumbnailTemp == null)
            {
                var img = transform.Find("InfoScrollView/Viewport/Content/ImageContainer/Frame/Image");
                if (img != null)
                {
                    _imageThumbnailTemp = img.GetComponent<Image>();
                }
            }

            if (_thumbnaiContainer == null)
            {
                var frame = transform.Find("InfoScrollView/Viewport/Content/ImageContainer/Frame");
                if (frame != null)
                {
                    _thumbnaiContainer = frame as RectTransform;
                }
            }

            if (_buttonDeleteThumbnail == null)
            {
                var del = transform.Find("InfoScrollView/Viewport/Content/ImageContainer/Frame/Button");
                if (del != null)
                {
                    _buttonDeleteThumbnail = del.GetComponent<Button>();
                }
            }

            if (TabSwitcher != null)
            {
                TabSwitcher.OnTabSwitched += OnTabSwitched;
            }
        }

        protected void Start()
        {
            InitializeDropdowns();
        }

        public void InitializeDropdowns()
        {
            if (dropdownVisibility == null || dropdownPublicationStatus == null)
            {
                var allDropdowns = GetComponentsInChildren<TMP_Dropdown>(true);
                foreach (var d in allDropdowns)
                {
                    if (d.name == "DropdownVisibility" || d.name == "Dropdown_Visibility")
                    {
                        dropdownVisibility = d;
                    }
                    else if (d.name == "DropdownPublicationStatus" || d.name == "Dropdown_PublicationStatus")
                    {
                        dropdownPublicationStatus = d;
                    }
                    else if (d.name == "Dropdown_GrantType" || d.name == "DropdownGrantType")
                    {
                        _dropdownGrantType = d;
                    }
                    else if (d.name == "Dropdown_GrantPrivileges" || d.name == "DropdownGrantPrivileges")
                    {
                        _dropdownGrantPrivileges = d;
                    }
                }
            }

            SetupDropdown(dropdownVisibility, new System.Collections.Generic.List<string> { "Private", "Protected", "Unlisted", "Public" });
            SetupDropdown(dropdownPublicationStatus, new System.Collections.Generic.List<string> { "Draft", "Published" });
            SetupDropdown(_dropdownGrantType, new System.Collections.Generic.List<string> { "User", "Group" });
            SetupDropdown(_dropdownGrantPrivileges, new System.Collections.Generic.List<string> { "Read", "Read & Update" });
        }

        private void OnTabSwitched(int index)
        {
            if (index == 1)
            {
                ResetInfoScrollPosition();
            }
        }

        private void OnDestroy()
        {
            if (_tabSwitcher != null)
            {
                _tabSwitcher.OnTabSwitched -= OnTabSwitched;
            }
        }

        private void SetupDropdown(TMP_Dropdown dropdown, System.Collections.Generic.List<string> options)
        {
            if (dropdown == null) return;
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
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
            if (dropdownVisibility != null)
            {
                dropdownVisibility.SetValueWithoutNotify(value);
                dropdownVisibility.RefreshShownValue();
            }
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
            if (dropdownPublicationStatus != null)
            {
                dropdownPublicationStatus.SetValueWithoutNotify(value);
                dropdownPublicationStatus.RefreshShownValue();
            }
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
        public bool IsAddGrantPanelActive() => _panelAddGrant != null && _panelAddGrant.activeSelf;
        public void ToggleAddGrantPanel()
        {
            if (_panelAddGrant != null)
            {
                _panelAddGrant.SetActive(!_panelAddGrant.activeSelf);
                if (_panelAddGrant.activeSelf && _inputFieldGranteeId != null)
                {
                    _inputFieldGranteeId.ActivateInputField();
                }
            }
        }

        public void HighlightGranteeInputError()
        {
            if (_inputFieldGranteeId != null)
            {
                _inputFieldGranteeId.ActivateInputField();
                if (_inputFieldGranteeId.placeholder is TMP_Text placeholderText)
                {
                    placeholderText.text = "Enter User or Group ID...";
                    placeholderText.color = new Color(1f, 0.4f, 0.4f, 0.9f);
                }
            }
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
            if (_inputFieldGranteeId != null)
            {
                _inputFieldGranteeId.text = string.Empty;
                if (_inputFieldGranteeId.placeholder is TMP_Text placeholderText)
                {
                    placeholderText.text = "Grantee ID...";
                    placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
                }
            }
        }

        public void SetGrantsList(System.Collections.Generic.List<LearningExperienceEngine.DataModel.Grant> grants, bool isEditor, bool canShare, UnityAction<int> onRemoveGrant)
        {
            if (_grantsContainer == null) return;

            foreach (Transform child in _grantsContainer)
            {
                Destroy(child.gameObject);
            }

            var headerObj = new GameObject("HeaderGrants", typeof(RectTransform));
            headerObj.transform.SetParent(_grantsContainer, false);
            var headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "Grants";
            headerText.fontSize = 16;
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = Color.white;

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

                if (canShare)
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

            LayoutRebuilder.ForceRebuildLayoutImmediate(_grantsContainer as RectTransform);
            if (_grantsContainer.parent is RectTransform parentRt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
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

        public void SetButtonThumbnailActive(bool active)
        {
            if (_buttonThumbnail != null)
            {
                _buttonThumbnail.gameObject.SetActive(active);
            }
        }

        public void SetThumbnailContainerActive(bool active)
        {
            if (_thumbnaiContainer != null)
            {
                _thumbnaiContainer.gameObject.SetActive(active);
            }
        }
        public void SetActionOnButtonNextStepClick(UnityAction action) => _nextStep.SafeSetListener(action);
        public void SetActionOnButtonPreviousStepClick(UnityAction action) => _previousStep.SafeSetListener(action);
        public void RemoveActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeRemoveListener(action);
        public void SetActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeAddListener(action);
        public void SetIsToggleEditorOn(bool value) => _toggleEditorMode.SafeSetIsOn(value);
        public void SetToggleEditorModeActive(bool active)
        {
            if (_toggleEditorMode != null) _toggleEditorMode.gameObject.SetActive(active);
        }

        public void SetCreatorAttributionText(string text, bool visible)
        {
            if (textCreatorAttribution != null)
            {
                textCreatorAttribution.text = text;
                textCreatorAttribution.gameObject.SetActive(visible);
            }
        }

        public void SetLicenseToggles(bool by, bool sa, bool nd, bool nc, bool interactable)
        {
            if (toggleLicenseBY != null) { toggleLicenseBY.SetIsOnWithoutNotify(by); toggleLicenseBY.interactable = interactable; }
            if (toggleLicenseSA != null) { toggleLicenseSA.SetIsOnWithoutNotify(sa); toggleLicenseSA.interactable = interactable; }
            if (toggleLicenseND != null) { toggleLicenseND.SetIsOnWithoutNotify(nd); toggleLicenseND.interactable = interactable; }
            if (toggleLicenseNC != null) { toggleLicenseNC.SetIsOnWithoutNotify(nc); toggleLicenseNC.interactable = interactable; }
        }

        public void SetActionOnToggleLicenseBY(UnityAction<bool> action) => toggleLicenseBY?.SafeAddListener(action);
        public void SetActionOnToggleLicenseSA(UnityAction<bool> action) => toggleLicenseSA?.SafeAddListener(action);
        public void SetActionOnToggleLicenseND(UnityAction<bool> action) => toggleLicenseND?.SafeAddListener(action);
        public void SetActionOnToggleLicenseNC(UnityAction<bool> action) => toggleLicenseNC?.SafeAddListener(action);
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
                _imageThumbnailTemp.gameObject.SetActive(value && _imageThumbnailTemp.sprite != null);
            }
        }
        //public void SetImageThumbnailTexture(Texture2D texture2D) => _imageThumbnailTemp.texture = texture2D;
        public void SetImageThumbnailTexture(Sprite sprite)
        {
            if (_imageThumbnailTemp != null)
            {
                _imageThumbnailTemp.sprite = sprite;
                _imageThumbnailTemp.gameObject.SetActive(sprite != null);
            }
        }
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
            ResetInfoScrollPosition();
        }

        public void ResetInfoScrollPosition()
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ResetInfoScrollPositionCoroutine());
            }
            else
            {
                ApplyScrollToTop();
            }
        }

        private IEnumerator ResetInfoScrollPositionCoroutine()
        {
            ApplyScrollToTop();
            yield return null;
            Canvas.ForceUpdateCanvases();
            ApplyScrollToTop();
        }

        private void ApplyScrollToTop()
        {
            var scroll = InfoScrollRect;
            if (scroll != null)
            {
                if (scroll.content != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
                    var pos = scroll.content.anchoredPosition;
                    pos.y = 0f;
                    scroll.content.anchoredPosition = pos;
                }
                scroll.velocity = Vector2.zero;
                scroll.verticalNormalizedPosition = 1f;
            }
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
