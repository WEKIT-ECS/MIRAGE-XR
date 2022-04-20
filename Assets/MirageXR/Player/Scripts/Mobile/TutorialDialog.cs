using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR {
    public class TutorialDialog : MonoBehaviour
    {
        [SerializeField] private Button _btnStartEditTutorial;
        [SerializeField] private Button _btnStartPlayTutorial;
        [SerializeField] private Button _btnCloseDialog;

        public void Init()
        {
            this.gameObject.SetActive(false);
            _btnStartEditTutorial.onClick.AddListener(OnStartEditTutorialClick);
            _btnStartPlayTutorial.onClick.AddListener(OnStartPlayTutorialClick);
            _btnCloseDialog.onClick.AddListener(OnCloseDialogClick);
        }

        public void Toggle()
        {
            if (!this.gameObject.activeInHierarchy)
            {
                this.gameObject.SetActive(true);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }

        private void OnStartEditTutorialClick()
        {
            this.gameObject.SetActive(false);
            TutorialManager.Instance.StartTutorial(TutorialManager.TutorialType.MOBILE_EDITING);
        }

        private void OnStartPlayTutorialClick()
        {
            this.gameObject.SetActive(false);
            TutorialManager.Instance.StartTutorial(TutorialManager.TutorialType.MOBILE_VIEWING);            
        }

        private void OnCloseDialogClick()
        {
            this.gameObject.SetActive(false);
        }
    }
}
