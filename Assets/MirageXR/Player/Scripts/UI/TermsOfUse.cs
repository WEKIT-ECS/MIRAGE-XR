using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using LearningExperienceEngine;

namespace MirageXR
{
    public class TermsOfUse : MonoBehaviour
    {

        [SerializeField] private GameObject TermsOfUseOfUsePanel;
        [SerializeField] private Sprite openIcon;
        [SerializeField] private Sprite closeIcon;
        [SerializeField] private Image buttonIcon;
        [SerializeField] private Text termsOfUseText;

        private LearningExperienceEngine.ConfigParser configParser = LearningExperienceEngine.LearningExperienceEngine.Instance.brandManager.CfgParser;

        private void Start()
        {
            termsOfUseText.supportRichText = true;

            //string TermsOfUse = configParser.Configuration.TermsOfUse;
            string TermsOfUse = string.Empty;
            if (Resources.Load<TextAsset>(configParser.Configuration.TermsOfUsePath))
                TermsOfUse = Resources.Load<TextAsset>(configParser.Configuration.TermsOfUsePath).text;

            if (TermsOfUse != string.Empty)
                termsOfUseText.text = TermsOfUse;
            else
                termsOfUseText.text = "Terms of use file could not be found!";

            if (PlayerPrefs.HasKey("termsOfUseRead"))
                Close();
            else
                Open();
        }

        public void ToggleTermsOfUse()
        {
            if (TermsOfUseOfUsePanel.activeInHierarchy)
            {

                if (!PlayerPrefs.HasKey("termsOfUseRead"))
                    PlayerPrefs.SetInt("termsOfUseRead", 1);

                Close();
            }
            else
                Open();

        }

        void Close()
        {
            buttonIcon.sprite = openIcon;
            TermsOfUseOfUsePanel.SetActive(false);
        }

        void Open()
        {
            buttonIcon.sprite = closeIcon;
            TermsOfUseOfUsePanel.SetActive(true);
        }

    }

}
