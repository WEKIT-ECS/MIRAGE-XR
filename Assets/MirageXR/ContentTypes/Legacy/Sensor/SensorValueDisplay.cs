using LearningExperienceEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SensorValueDisplay : MonoBehaviour
    {
        private Image _bg;
        private Text _valueText;
        public string Value
        {
            get; set;
        }

        [SerializeField] private Color32 Green;
        [SerializeField] private Color32 Yellow;
        [SerializeField] private Color32 Red;
        [SerializeField] private Color32 Normal;
        [SerializeField] private Color32 Disabled;

        private string state = "normal";

        // Use this for initialization
        void Awake()
        {
            _bg = GetComponent<Image>();
            _valueText = transform.FindDeepChild("ValueText").GetComponent<Text>();

            // Set default color to no limit.
            _bg.color = Normal;
        }

        public void SetState(string input)
        {
            state = input;
        }

        // Update is called once per frame
        void Update()
        {
            _valueText.text = Value;

            switch (state)
            {
                case "green":
                    _bg.color = Green;
                    break;
                case "yellow":
                    _bg.color = Yellow;
                    break;
                case "red":
                    _bg.color = Red;
                    break;
                case "disabled":
                    _bg.color = Disabled;
                    break;
                case "normal":
                    _bg.color = Normal;
                    break;
            }
        }
    }
}