using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class ProfileScreenSpatialView : ScreenView
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonLogin;
        [SerializeField] private Button _buttonRegister;
        [SerializeField] private Button _buttonMoodleServers;
        [SerializeField] private Button _buttonServer;
        [SerializeField] private Button _buttonSketchfab;
        [SerializeField] private Button _buttonServiceName;
        [SerializeField] private Button _buttonAvatar;
        [SerializeField] private Button _buttonGrid;
        [SerializeField] private Button _buttonAudioDevice;
        [SerializeField] private Button _buttonDevelopMode;
        [SerializeField] private Button _btnServerAddress;
        [SerializeField] private Button _btnServerPort;
        [SerializeField] private Button _btnWebsocketAddress;
        [SerializeField] private Button _btnWebsocketPort;
        [Header("Toggles")]
        [SerializeField] private Toggle _toggleConsole;
        [Header("Texts")]
        [SerializeField] private TMP_Text _txtServerAddress;
        [SerializeField] private TMP_Text _txtServerPort;
        [SerializeField] private TMP_Text _txtWebsocketAddress;
        [SerializeField] private TMP_Text _txtWebsocketPort;
        [SerializeField] private TMP_Text _tmpTextMoodleServer;
        [SerializeField] private TMP_Text _tmpTextServer;
        [SerializeField] private TMP_Text _tmpTextSketchfab;
        [Header("GameObjects")]
        [SerializeField] private GameObject _signInPrefab; // TEMP
        [SerializeField] private GameObject _registerPrefab;// TEMP
        [SerializeField] private AudioDeviceSpatialView _audioDevicePrefab;
        [SerializeField] private SketchfabSignInPopupView sketchfabSignInPopupViewPrefab;
        [SerializeField] private ChangeUserAvatarView _changeUserAvatarViewPrefab;
        public void SetActionOnButtonLoginClick(UnityAction action) => _buttonLogin.SafeSetListener(action);
        public void SetActionOnButtonServerAddressClick(UnityAction action) => _btnServerAddress.SafeSetListener(action);
        public void SetActionOnButtonServerPortClick(UnityAction action) => _btnServerPort.SafeSetListener(action);
        public void SetActionOnButtonWebsocketAddressClick(UnityAction action) => _btnWebsocketAddress.SafeSetListener(action);
        public void SetActionOnButtonWebsocketPortClick(UnityAction action) => _btnWebsocketPort.SafeSetListener(action);
        public void SetActionOnButtonRegisterClick(UnityAction action) => _buttonRegister.SafeSetListener(action);
        public void SetActionOnButtonMoodleServersClick(UnityAction action) => _buttonMoodleServers.SafeSetListener(action);
        public void SetActionOnButtonServerClick(UnityAction action) => _buttonServer.SafeSetListener(action);
        public void SetActionOnButtonSketchfabClick(UnityAction action) => _buttonSketchfab.SafeSetListener(action);
        public void SetActionOnButtonServiceNameClick(UnityAction action) => _buttonServiceName.SafeSetListener(action);
        public void SetActionOnButtonAvatarClick(UnityAction action) => _buttonAvatar.SafeSetListener(action);
        public void SetActionOnButtonGridClick(UnityAction action) => _buttonGrid.SafeSetListener(action);
        public void SetActionOnButtonAudioDeviceClick(UnityAction action) => _buttonAudioDevice.SafeSetListener(action);
        public void SetActionOnButtonDevelopModeClick(UnityAction action) => _buttonDevelopMode.SafeSetListener(action);
        public void SetActionOnToggleConsoleValueChanged(UnityAction<bool> action) => _toggleConsole.SafeSetListener(action);
        public void SetSketchfabText(string text) => _tmpTextSketchfab.SafeSetText(text);
        public void SetServerAddressText(string text) => _txtServerAddress.SafeSetText(text);
        public void SetServerPortText(string text) => _txtServerPort.SafeSetText(text);
        public void SetWebsocketAddressText(string text) => _txtWebsocketAddress.SafeSetText(text);
        public void SetWebsocketPortText(string text) => _txtWebsocketPort.SafeSetText(text);
        public AudioDeviceSpatialView GetAudioDevicePrefab() => _audioDevicePrefab;
        public SketchfabSignInPopupView GetSketchfabSignInPopupViewPrefab() => sketchfabSignInPopupViewPrefab;
        public ChangeUserAvatarView GetChangeUserAvatarViewPrefab() => _changeUserAvatarViewPrefab;

        public void ShowSignInPanel()
        {
            _signInPrefab.SetActive(true);
        }
    }
}
