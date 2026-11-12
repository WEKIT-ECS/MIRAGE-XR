using System;
using System.Globalization;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class RoomScanRepositionSpatialView : PopupBase
    {
        [SerializeField] private Button _btnClose;

        private TMP_InputField _inputOffsetX;
        private TMP_InputField _inputOffsetY;
        private TMP_InputField _inputOffsetZ;

        private TMP_InputField _inputRotX;
        private TMP_InputField _inputRotY;
        private TMP_InputField _inputRotZ;

        private TMP_InputField _inputScaleX;
        private TMP_InputField _inputScaleY;
        private TMP_InputField _inputScaleZ;

        private bool _isInitializedUI = false;
        private static RoomTwinManager roomTwinManager => RootObject.Instance?.RoomTwinManager;

        public void SetupCloseButton(Button closeButton)
        {
            _btnClose = closeButton;
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            if (_btnClose != null)
            {
                _btnClose.onClick.RemoveAllListeners();
                _btnClose.onClick.AddListener(Close);
            }

            BuildUIIfNeeded();
            LoadCurrentValues();
        }

        private void BuildUIIfNeeded()
        {
            if (_isInitializedUI) return;

            // Find Header title and update it
            var title = GetComponentInChildren<TMP_Text>(true);
            if (title != null)
            {
                title.text = "Reposition Digital Twin";
                title.gameObject.SetActive(true);
            }

            var rootRect = GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.sizeDelta = new Vector2(480f, 440f);
            }

            // Find Content container
            Transform contentTransform = transform.Find("Screen/Content");
            if (contentTransform == null)
            {
                contentTransform = transform;
            }

            // Hide or clean any existing style panel panels
            var stylePanel = contentTransform.Find("Panel_RoomScanStyle");
            if (stylePanel != null)
            {
                stylePanel.gameObject.SetActive(false);
            }

            // Create container for Reposition UI
            var repoContainer = new GameObject("RepositionContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
            repoContainer.transform.SetParent(contentTransform, false);
            var repoRect = repoContainer.GetComponent<RectTransform>();
            repoRect.anchorMin = new Vector2(0f, 0f);
            repoRect.anchorMax = new Vector2(1f, 1f);
            repoRect.offsetMin = new Vector2(20f, 20f);
            repoRect.offsetMax = new Vector2(-20f, -20f);

            var vlg = repoContainer.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Row 1: Offset (cm)
            CreateVector3Row(repoContainer.transform, "Offset from Anchor (cm):", "X", "Y", "Z",
                out _inputOffsetX, out _inputOffsetY, out _inputOffsetZ);

            // Row 2: Rotation (deg)
            CreateVector3Row(repoContainer.transform, "Rotation (deg):", "X", "Y", "Z",
                out _inputRotX, out _inputRotY, out _inputRotZ);

            // Row 3: Size / Scale
            CreateVector3Row(repoContainer.transform, "Size (Scale factor):", "X", "Y", "Z",
                out _inputScaleX, out _inputScaleY, out _inputScaleZ);

            // Row 4: Buttons (Reset / Apply)
            var buttonRow = new GameObject("ButtonRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.transform.SetParent(repoContainer.transform, false);
            var btnRowRect = buttonRow.GetComponent<RectTransform>();
            btnRowRect.sizeDelta = new Vector2(0f, 40f);

            var hlgBtn = buttonRow.GetComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 20f;
            hlgBtn.childControlWidth = false;
            hlgBtn.childControlHeight = true;
            hlgBtn.childForceExpandWidth = false;
            hlgBtn.childForceExpandHeight = true;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;

            CreateButton(buttonRow.transform, "Reset", OnResetClicked);
            CreateButton(buttonRow.transform, "Apply", OnApplyClicked);

            // Hook up onEndEdit to auto-apply
            _inputOffsetX.onEndEdit.AddListener(_ => OnApplyClicked());
            _inputOffsetY.onEndEdit.AddListener(_ => OnApplyClicked());
            _inputOffsetZ.onEndEdit.AddListener(_ => OnApplyClicked());

            _inputRotX.onEndEdit.AddListener(_ => OnApplyClicked());
            _inputRotY.onEndEdit.AddListener(_ => OnApplyClicked());
            _inputRotZ.onEndEdit.AddListener(_ => OnApplyClicked());

            _inputScaleX.onEndEdit.AddListener(_ => OnApplyClicked());
            _inputScaleY.onEndEdit.AddListener(_ => OnApplyClicked());
            _inputScaleZ.onEndEdit.AddListener(_ => OnApplyClicked());

            _isInitializedUI = true;
        }

        private void LoadCurrentValues()
        {
            var rtm = roomTwinManager;
            var loc = rtm != null ? rtm.GetRoomTwinLocation() : Location.GetIdentityLocation();

            // Position in meters -> cm
            float cmX = loc.Position.x * 100f;
            float cmY = loc.Position.y * 100f;
            float cmZ = loc.Position.z * 100f;

            if (_inputOffsetX != null) _inputOffsetX.text = cmX.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputOffsetY != null) _inputOffsetY.text = cmY.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputOffsetZ != null) _inputOffsetZ.text = cmZ.ToString("0.##", CultureInfo.InvariantCulture);

            // Rotation
            if (_inputRotX != null) _inputRotX.text = loc.Rotation.x.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputRotY != null) _inputRotY.text = loc.Rotation.y.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputRotZ != null) _inputRotZ.text = loc.Rotation.z.ToString("0.##", CultureInfo.InvariantCulture);

            // Scale
            Vector3 scale = loc.Scale == Vector3.zero ? Vector3.one : loc.Scale;
            if (_inputScaleX != null) _inputScaleX.text = scale.x.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputScaleY != null) _inputScaleY.text = scale.y.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputScaleZ != null) _inputScaleZ.text = scale.z.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void OnApplyClicked()
        {
            var rtm = roomTwinManager;
            if (rtm == null) return;

            float posX = ParseFloat(_inputOffsetX?.text, 0f) * 0.01f;
            float posY = ParseFloat(_inputOffsetY?.text, 0f) * 0.01f;
            float posZ = ParseFloat(_inputOffsetZ?.text, 0f) * 0.01f;

            float rotX = ParseFloat(_inputRotX?.text, 0f);
            float rotY = ParseFloat(_inputRotY?.text, 0f);
            float rotZ = ParseFloat(_inputRotZ?.text, 0f);

            float scaleX = ParseFloat(_inputScaleX?.text, 1f);
            float scaleY = ParseFloat(_inputScaleY?.text, 1f);
            float scaleZ = ParseFloat(_inputScaleZ?.text, 1f);

            if (scaleX == 0f) scaleX = 1f;
            if (scaleY == 0f) scaleY = 1f;
            if (scaleZ == 0f) scaleZ = 1f;

            var newLocation = new Location
            {
                Position = new Vector3(posX, posY, posZ),
                Rotation = new Vector3(rotX, rotY, rotZ),
                Scale = new Vector3(scaleX, scaleY, scaleZ)
            };

            rtm.UpdateAndPersistLocation(newLocation);
        }

        private void OnResetClicked()
        {
            var rtm = roomTwinManager;
            if (rtm == null) return;

            var identity = Location.GetIdentityLocation();
            rtm.UpdateAndPersistLocation(identity);
            LoadCurrentValues();
        }

        private static float ParseFloat(string text, float fallback)
        {
            if (string.IsNullOrWhiteSpace(text)) return fallback;
            if (float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return fallback;
        }

        private static void CreateVector3Row(Transform parent, string title, string labelX, string labelY, string labelZ,
            out TMP_InputField outX, out TMP_InputField outY, out TMP_InputField outZ)
        {
            var groupGo = new GameObject("RowGroup_" + title, typeof(RectTransform), typeof(VerticalLayoutGroup));
            groupGo.transform.SetParent(parent, false);

            var vlg = groupGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(groupGo.transform, false);
            var titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
            titleTxt.fontSize = 17f;
            titleTxt.color = Color.white;
            titleTxt.text = title;
            titleTxt.alignment = TextAlignmentOptions.Left;

            var rowGo = new GameObject("InputsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowGo.transform.SetParent(groupGo.transform, false);
            var rowRect = rowGo.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 36f);

            var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            outX = CreateNamedInput(rowGo.transform, labelX);
            outY = CreateNamedInput(rowGo.transform, labelY);
            outZ = CreateNamedInput(rowGo.transform, labelZ);
        }

        private static TMP_InputField CreateNamedInput(Transform parent, string nameLabel)
        {
            var pairGo = new GameObject("Pair_" + nameLabel, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            pairGo.transform.SetParent(parent, false);
            var pairRect = pairGo.GetComponent<RectTransform>();
            pairRect.sizeDelta = new Vector2(120f, 36f);

            var hlg = pairGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var lblGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGo.transform.SetParent(pairGo.transform, false);
            var lblTxt = lblGo.GetComponent<TextMeshProUGUI>();
            lblTxt.fontSize = 16f;
            lblTxt.color = new Color(0.8f, 0.8f, 0.9f);
            lblTxt.text = nameLabel + ":";
            lblTxt.rectTransform.sizeDelta = new Vector2(20f, 36f);
            lblTxt.alignment = TextAlignmentOptions.MidlineLeft;

            return CreateInputField(pairGo.transform, "0.0", "0.0");
        }

        private static TMP_InputField CreateInputField(Transform parent, string placeholderText, string initialValue)
        {
            var go = new GameObject("InputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90f, 34f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.12f, 0.12f, 0.16f, 0.85f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 2f);
            textRect.offsetMax = new Vector2(-8f, -2f);

            var textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.fontSize = 16f;
            textComp.color = Color.white;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;

            var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            phGo.transform.SetParent(go.transform, false);
            var phRect = phGo.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(8f, 2f);
            phRect.offsetMax = new Vector2(-8f, -2f);

            var phComp = phGo.GetComponent<TextMeshProUGUI>();
            phComp.fontSize = 16f;
            phComp.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
            phComp.text = placeholderText;
            phComp.alignment = TextAlignmentOptions.MidlineLeft;

            var input = go.GetComponent<TMP_InputField>();
            input.textViewport = rect;
            input.textComponent = textComp;
            input.placeholder = phComp;
            input.contentType = TMP_InputField.ContentType.DecimalNumber;
            input.text = initialValue;

            return input;
        }

        private static Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Button_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 38f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.25f, 0.35f, 0.5f, 0.9f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComp = textGo.GetComponent<TextMeshProUGUI>();
            textComp.fontSize = 16f;
            textComp.color = Color.white;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.text = label;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            return btn;
        }
    }
}

