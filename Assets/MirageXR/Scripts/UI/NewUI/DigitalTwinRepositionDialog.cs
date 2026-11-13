using System;
using System.Globalization;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class DigitalTwinRepositionDialog : MonoBehaviour
    {
        [Header("Position Inputs (cm)")]
        [SerializeField] private TMP_InputField _inputPosX;
        [SerializeField] private TMP_InputField _inputPosY;
        [SerializeField] private TMP_InputField _inputPosZ;

        [Header("Rotation Inputs (deg)")]
        [SerializeField] private TMP_InputField _inputRotX;
        [SerializeField] private TMP_InputField _inputRotY;
        [SerializeField] private TMP_InputField _inputRotZ;

        [Header("Scale Inputs")]
        [SerializeField] private TMP_InputField _inputScaleX;
        [SerializeField] private TMP_InputField _inputScaleY;
        [SerializeField] private TMP_InputField _inputScaleZ;

        [Header("Buttons")]
        [SerializeField] private Button _btnReset;
        [SerializeField] private Button _btnCancel;
        [SerializeField] private Button _btnApply;
        [SerializeField] private Button _btnBackgroundOverlay;

        private RoomTwinManager _rtm;
        private Action _onClose;

        public void Init(RoomTwinManager rtm, Action onClose)
        {
            _rtm = rtm ?? RootObject.Instance?.RoomTwinManager;
            _onClose = onClose;

            if (_btnReset != null)
            {
                _btnReset.onClick.RemoveAllListeners();
                _btnReset.onClick.AddListener(OnResetClicked);
            }

            if (_btnCancel != null)
            {
                _btnCancel.onClick.RemoveAllListeners();
                _btnCancel.onClick.AddListener(Close);
            }

            if (_btnApply != null)
            {
                _btnApply.onClick.RemoveAllListeners();
                _btnApply.onClick.AddListener(OnApplyClicked);
            }

            if (_btnBackgroundOverlay != null)
            {
                _btnBackgroundOverlay.onClick.RemoveAllListeners();
                _btnBackgroundOverlay.onClick.AddListener(Close);
            }

            ConfigureInputFields();
            LoadCurrentValues();
        }

        private void ConfigureInputFields()
        {
            void SetupField(TMP_InputField field, string placeholder)
            {
                if (field == null) return;
                field.contentType = TMP_InputField.ContentType.Standard;
                field.keyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                field.characterLimit = 15;
                if (field.placeholder is TMP_Text ph)
                {
                    ph.text = placeholder;
                }
            }

            SetupField(_inputPosX, "X");
            SetupField(_inputPosY, "Y");
            SetupField(_inputPosZ, "Z");

            SetupField(_inputRotX, "X");
            SetupField(_inputRotY, "Y");
            SetupField(_inputRotZ, "Z");

            SetupField(_inputScaleX, "1");
            SetupField(_inputScaleY, "1");
            SetupField(_inputScaleZ, "1");
        }

        public void LoadCurrentValues()
        {
            var loc = _rtm != null ? _rtm.GetRoomTwinLocation() : Location.GetIdentityLocation();
            if (loc == null) loc = Location.GetIdentityLocation();

            float posXcm = loc.Position.x * 100f;
            float posYcm = loc.Position.y * 100f;
            float posZcm = loc.Position.z * 100f;

            if (_inputPosX != null) _inputPosX.text = posXcm.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputPosY != null) _inputPosY.text = posYcm.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputPosZ != null) _inputPosZ.text = posZcm.ToString("0.##", CultureInfo.InvariantCulture);

            Vector3 rot = loc.Rotation;
            if (_inputRotX != null) _inputRotX.text = rot.x.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputRotY != null) _inputRotY.text = rot.y.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputRotZ != null) _inputRotZ.text = rot.z.ToString("0.##", CultureInfo.InvariantCulture);

            Vector3 scale = loc.Scale == Vector3.zero ? Vector3.one : loc.Scale;
            if (_inputScaleX != null) _inputScaleX.text = scale.x.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputScaleY != null) _inputScaleY.text = scale.y.ToString("0.##", CultureInfo.InvariantCulture);
            if (_inputScaleZ != null) _inputScaleZ.text = scale.z.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void OnResetClicked()
        {
            if (_inputPosX != null) _inputPosX.text = "0";
            if (_inputPosY != null) _inputPosY.text = "0";
            if (_inputPosZ != null) _inputPosZ.text = "0";

            if (_inputRotX != null) _inputRotX.text = "0";
            if (_inputRotY != null) _inputRotY.text = "0";
            if (_inputRotZ != null) _inputRotZ.text = "0";

            if (_inputScaleX != null) _inputScaleX.text = "1";
            if (_inputScaleY != null) _inputScaleY.text = "1";
            if (_inputScaleZ != null) _inputScaleZ.text = "1";
        }

        private void OnApplyClicked()
        {
            if (_rtm == null) return;

            float px = ParseInvariantFloat(_inputPosX?.text, 0f) * 0.01f;
            float py = ParseInvariantFloat(_inputPosY?.text, 0f) * 0.01f;
            float pz = ParseInvariantFloat(_inputPosZ?.text, 0f) * 0.01f;

            float rx = ParseInvariantFloat(_inputRotX?.text, 0f);
            float ry = ParseInvariantFloat(_inputRotY?.text, 0f);
            float rz = ParseInvariantFloat(_inputRotZ?.text, 0f);

            float sx = ParseInvariantFloat(_inputScaleX?.text, 1f);
            float sy = ParseInvariantFloat(_inputScaleY?.text, 1f);
            float sz = ParseInvariantFloat(_inputScaleZ?.text, 1f);

            Vector3 newPos = new Vector3(px, py, pz);
            Vector3 newScale = new Vector3(sx, sy, sz);
            if (newScale.sqrMagnitude < 0.0001f) newScale = Vector3.one;

            var newLocation = new Location
            {
                Position = newPos,
                Rotation = new Vector3(rx, ry, rz),
                Scale = newScale
            };

            _rtm.UpdateAndPersistLocation(newLocation);
            Close();
        }

        public void Close()
        {
            _onClose?.Invoke();
            Destroy(gameObject);
        }

        private static float ParseInvariantFloat(string s, float fallback)
        {
            if (string.IsNullOrWhiteSpace(s)) return fallback;
            s = s.Trim();
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
            {
                return val;
            }
            return fallback;
        }
    }
}

