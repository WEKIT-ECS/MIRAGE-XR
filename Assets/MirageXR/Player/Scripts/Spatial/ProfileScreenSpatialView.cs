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
        [SerializeField] private Button _buttonDevelopMode;
        [Header("Texts")]
        [SerializeField] private TMP_Text _tmpTextMoodleServer;
        [SerializeField] private TMP_Text _tmpTextServer;

#if UNITY_VISIONOS
        public void SetActionOnButtonLoginClick(UnityAction action) => _buttonLogin.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonRegisterClick(UnityAction action) => _buttonRegister.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonMoodleServersClick(UnityAction action) => _buttonMoodleServers.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonServerClick(UnityAction action) => _buttonServer.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonSketchfabClick(UnityAction action) => _buttonSketchfab.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonServiceNameClick(UnityAction action) => _buttonServiceName.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonAvatarClick(UnityAction action) => _buttonAvatar.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonGridClick(UnityAction action) => _buttonGrid.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);
        public void SetActionOnButtonDevelopModeClick(UnityAction action) => _buttonDevelopMode.gameObject.AddComponent<VisionOSButtonInteraction>().onClick.AddListener(action);

#else
        
        public void SetActionOnButtonLoginClick(UnityAction action) => _buttonLogin.SafeSetListener(action);
        public void SetActionOnButtonRegisterClick(UnityAction action) => _buttonRegister.SafeSetListener(action);
        public void SetActionOnButtonMoodleServersClick(UnityAction action) => _buttonMoodleServers.SafeSetListener(action);
        public void SetActionOnButtonServerClick(UnityAction action) => _buttonServer.SafeSetListener(action);
        public void SetActionOnButtonSketchfabClick(UnityAction action) => _buttonSketchfab.SafeSetListener(action);
        public void SetActionOnButtonServiceNameClick(UnityAction action) => _buttonServiceName.SafeSetListener(action);
        public void SetActionOnButtonAvatarClick(UnityAction action) => _buttonAvatar.SafeSetListener(action);
        public void SetActionOnButtonGridClick(UnityAction action) => _buttonGrid.SafeSetListener(action);
        public void SetActionOnButtonDevelopModeClick(UnityAction action) => _buttonDevelopMode.SafeSetListener(action);

#endif
    }
}
