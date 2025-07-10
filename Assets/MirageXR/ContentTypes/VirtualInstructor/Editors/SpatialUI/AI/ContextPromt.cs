using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ContextPromt : PopupBase
    {
        [SerializeField] private Button close;
        [SerializeField] private Button accept;
        [SerializeField] private Button openTooltipBtn; 
        [SerializeField] private Button closeTooltipBtn; 
        [SerializeField] private GameObject openTooltip; 
        [SerializeField] private GameObject closeTooltip;
        [SerializeField] private TMP_InputField prompt;

        private string _prompt;
        private Action<string> _callback;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            close.onClick.AddListener(OnClose);
            accept.onClick.AddListener(OnAccept);
            openTooltipBtn.onClick.AddListener(ShowToolTip);
            closeTooltipBtn.onClick.AddListener(HideToolTip);
            prompt.onValueChanged.AddListener(newValue => {_prompt = newValue; });
        }

        private void OnClose()
        {
            Close();
        }

        private void OnAccept()
        {
            Close();
            _callback.Invoke(_prompt);
        }

        private void ShowToolTip()
        {
            closeTooltip.SetActive(true);
            openTooltip.SetActive(false);
        }

        private void HideToolTip()
        {
            closeTooltip.SetActive(false);
            openTooltip.SetActive(true);
        }

        private void SetPrompt(string promptText)
        {
            _prompt = promptText;
            prompt.text = _prompt;
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            if (args is not { Length: 2 })
            {
                return false;
            }

            if (args[0] is not string promptText || args[1] is not Action<string> callback)
            {
                return false;
            }

            SetPrompt(promptText);
            _callback = callback;
            return true;
        }
    }
}
