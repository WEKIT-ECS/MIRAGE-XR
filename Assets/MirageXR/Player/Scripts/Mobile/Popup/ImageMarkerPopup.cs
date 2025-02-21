using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

namespace MirageXR
{

    public class ImageMarkerPopup : PopupBase
    {
        [SerializeField] private Button _btnGotIt;
        [SerializeField] private TMP_Text _txtInstruction;
        [SerializeField] private Image _imgMarkerImage;

        public void SetImage(String url)
        {
            if (url.StartsWith("resources://"))
            {
                url = url.Replace("resources://", "");
            }

            byte[] byteArray = File.ReadAllBytes(Path.Combine(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath, url));

            Texture2D loadTexture = new Texture2D(2, 2);

            bool isLoaded = loadTexture.LoadImage(byteArray);

            if (isLoaded)
            {
                var sprite = Utilities.TextureToSprite(loadTexture);
                _imgMarkerImage.sprite = sprite;
                _imgMarkerImage.gameObject.SetActive(true);
            }
            else
            {
                _imgMarkerImage.gameObject.SetActive(false);
                _txtInstruction.text = "Image not found at: \n" + LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath + url;
            }
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnGotIt.onClick.AddListener(GotItOnClick);
        }

        private void GotItOnClick()
        {
            EventManager.NotifyOnTutorialPopupCloseClicked();
            Close();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
    }
}
