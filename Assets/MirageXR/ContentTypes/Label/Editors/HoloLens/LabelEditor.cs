using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


namespace MirageXR
{
    public class LabelEditor : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private InputField textInputField;
        [SerializeField] private Transform annotationStartingPoint;
        [SerializeField] private LearningExperienceEngine.StepTrigger stepTrigger;
        [SerializeField] private GameObject acceptButton;

        [SerializeField] private GameObject _settingsPannel;
        [SerializeField] private GameObject _textPannel;

        [SerializeField] private TMP_Text _exampleLabel;
        [SerializeField] private Image _exampleLabelBackground;
        [SerializeField] private TMP_InputField _fontSize;

        [SerializeField] private Image _fontColourButtonImage;
        [SerializeField] private Image _backgroundColourButtonImage;

        [SerializeField] private ColourSelector _colourPickerScript;

        private enum ColourPickerOption { NA, Font, Background};

        private ColourPickerOption _colourPickerOption = ColourPickerOption.NA;



        public GameObject AcceptButton => acceptButton;

        private LearningExperienceEngine.Action action;
        private LearningExperienceEngine.ToggleObject annotationToEdit;

        public void Close()
        {
            action = null;
            annotationToEdit = null;
            gameObject.SetActive(false);
            this.textInputField.onValueChanged.RemoveListener(delegate { EventManager.NotifyOnLabelEditorTextChanged(); });

            Destroy(gameObject);
        }

        public void SetAnnotationStartingPoint(Transform startingPoint)
        {
            annotationStartingPoint = startingPoint;
        }

        public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
        {
            _colourPickerScript.onColourSelected.AddListener(OnColourPickerChange);

            gameObject.SetActive(true);
            this.action = action;
            annotationToEdit = annotation;

            if (annotationToEdit != null)
            {
                textInputField.text = annotationToEdit.text;
                var trigger = activityManager.ActiveAction.triggers.Find(t => t.id == annotationToEdit.poi);
                var duration = trigger != null ? trigger.duration : 1;
                var stepNumber = trigger != null ? trigger.value : "1";
                stepTrigger.Initiate(annotationToEdit, duration, stepNumber);

                if (annotationToEdit.option != "")
                {
                    string[] splitArray = annotationToEdit.option.Split(char.Parse("-"));

                    _exampleLabel.text = annotationToEdit.text;

                    _exampleLabel.fontSize = int.Parse(splitArray[0]);

                    _exampleLabel.color = GetColorFromString(splitArray[1]);
                    _exampleLabelBackground.color = GetColorFromString(splitArray[2]);
                }
            }

            UpdateButtonColours();

            this.textInputField.onValueChanged.AddListener(delegate { EventManager.NotifyOnLabelEditorTextChanged(); });
          //  this.acceptButton = this.gameObject.transform.Find("AcceptButton").gameObject;
        }

        public void OnAccept()
        {
            if (string.IsNullOrEmpty(textInputField.text))
            {
                Toast.Instance.Show("Input field is empty.");
                return;
            }

            if (annotationToEdit != null)
            {
                LearningExperienceEngine.EventManager.DeactivateObject(annotationToEdit);
            }
            else
            {
                var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
                LearningExperienceEngine.Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
                GameObject originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                    annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(action, offset);
                annotationToEdit.predicate = "label";
            }
            annotationToEdit.text = textInputField.text;

            annotationToEdit.option = _exampleLabel.fontSize.ToString() + "-" + _exampleLabel.color.ToString() + "-" + _exampleLabelBackground.color.ToString();

            stepTrigger.MyPoi = annotationToEdit;
            stepTrigger.SetupTrigger();

            LearningExperienceEngine.EventManager.ActivateObject(annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(action);
            Close();
        }

        public void OpenSettings(bool open)
        {
            _settingsPannel.SetActive(open);
            _textPannel.SetActive(!open);
        }

        public void OnFontSizeChanged()
        {
            _exampleLabel.fontSize = int.Parse(_fontSize.text);
        }


        public void OnFontColourChange()
        {
           // _colourPickerObject.SetActive(true);
            _colourPickerScript.Open();
            _colourPickerOption = ColourPickerOption.Font;
        }

        public void OnBackgroundColourChanged()
        {
            //_colourPickerObject.SetActive(true);
            _colourPickerScript.Open();
            _colourPickerOption = ColourPickerOption.Background;
        }

        public void OnColourPickerChange()
        {
            switch (_colourPickerOption)
            {
                case ColourPickerOption.Font:
                    _exampleLabel.color = _colourPickerScript._selectedColour;
                    break;
                case ColourPickerOption.Background:
                    _exampleLabelBackground.color = _colourPickerScript._selectedColour;
                    break;
                default:
                    break;
            }

            UpdateButtonColours();

            _colourPickerOption = ColourPickerOption.NA;
        }

        private Color GetColorFromString(string rgb)
        {
            string[] rgba = rgb.Substring(5, rgb.Length - 6).Split(", ");
            Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));

            return color;
        }

        public void onInputChanged()
        {
            _exampleLabel.text = textInputField.text;
        }

        private void UpdateButtonColours()
        {
            _fontColourButtonImage.color = _exampleLabel.color;
            _backgroundColourButtonImage.color = _exampleLabelBackground.color;
        }
    }
}
