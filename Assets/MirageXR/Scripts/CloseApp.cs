using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class CloseApp : MonoBehaviour
    {
        [SerializeField] private bool closeButtonShouldCloseApp = false;
        [Space]
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject popupConfirmation;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private List<GameObject> controls;
    
        private bool confirmationOpen = false;


        private void Start()
        {
            if (closeButtonShouldCloseApp == true)
            {
                closeButton.onClick.AddListener(ShowConfirmation);
                cancelButton.onClick.AddListener(HideConfirmation);
                confirmButton.onClick.AddListener(LogoutAndExit);
            }
        }

        private void ShowConfirmation()
        {
            popupConfirmation.SetActive(true);
            confirmationOpen = true;
            foreach (GameObject control in controls)
            {
                control.SetActive(false);
            }
        }

        private void HideConfirmation()
        {
            foreach (GameObject control in controls)
            {
                control.SetActive(true);
            }
            confirmationOpen = false;
            popupConfirmation.SetActive(false);
        }

        private void LogoutAndExit()
        {
            if (!confirmationOpen)
            {
                return;
            }

            confirmationOpen = false;
            confirmButton.interactable = false;

            var authorizationManager = LearningExperienceEngine.LearningExperienceEngine.Instance?.AuthorizationManager;
            if (authorizationManager != null && authorizationManager.LoggedIn())
            {
                authorizationManager.Logout();
            }

            LearningExperienceEngine.UserSettings.ClearLoginData();
            Application.Quit();
        }

    }
}
