using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class TwoStateButton_Sidebar : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _iconButtonOn;
        [SerializeField] private GameObject _iconButtonOff;
        [SerializeField] private GameObject _content;
        
        private void Awake()
        {
            SetState(true);
            _button.onClick.AddListener(OnButtonClicked); 
        }

        private void OnButtonClicked()
        {
            SetState(!_content.activeSelf);
        }

        private void SetState(bool isActive)
        {
            _content.SetActive(isActive);
            _iconButtonOn.SetActive(isActive);
            _iconButtonOff.SetActive(!isActive);
        }
    }
}
