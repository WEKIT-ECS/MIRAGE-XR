using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SensorDisplay : MonoBehaviour
    {
        private Text _label;

        private GameObject _indicator;

        public string Key;
        public string Name;

        public enum SensorValueType
        {
            Int,
            Float
        }

        public SensorValueType Type = SensorValueType.Float;

        private RectTransform _redMin;
        private RectTransform _redMax;

        public float YellowLimitMin = 25;
        private RectTransform _yelMin;
        public float YellowLimitMax = 75;
        private RectTransform _yelMax;

        public float GreenLimitMin = 45;
        public float GreenLimitMax = 65;

        public enum SensorCategory
        {
            Human,
            Environment
        }

        public SensorCategory Category = SensorCategory.Human;

        public enum SensorState
        {
            Red,
            Yellow,
            Green
        }

        public SensorState State = SensorState.Green;

        public float Min;
        public float Max = 100;

        public string Value;

        public string Unit;

        private void OnEnable()
        {
            EventManager.OnClearAll += Delete;
        }

        private void OnDisable()
        {
            EventManager.OnClearAll -= Delete;
        }

        private void Awake()
        {
            // Assign objects.
            _label = transform.FindDeepChild("Label").GetComponent<Text>();
            _indicator = transform.FindDeepChild("Indicator").gameObject;

            _redMin = transform.FindDeepChild("RedLow").GetComponent<RectTransform>();
            _redMax = transform.FindDeepChild("RedHigh").GetComponent<RectTransform>();
            _yelMin = transform.FindDeepChild("YellowLow").GetComponent<RectTransform>();
            _yelMax = transform.FindDeepChild("YellowHigh").GetComponent<RectTransform>();
        }

        private void Delete()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            _label.text = Name;
            var redMinSize = (YellowLimitMin - Min) / (Max - Min) * 700;
            redMinSize += redMinSize/2;
            if (redMinSize > 700)
                redMinSize = 700;
            _redMin.sizeDelta = new Vector2(redMinSize, 24f);

            var yellowMinSize = (GreenLimitMin - Min) / (Max - Min) * 700;
            yellowMinSize += yellowMinSize / 2;
            if (yellowMinSize > 700)
                yellowMinSize = 700;
            _yelMin.sizeDelta = new Vector2(yellowMinSize, 24f);

            var redMaxSize = (Max - YellowLimitMax) / (Max - Min) * 700;
            redMaxSize += redMaxSize / 2;
            if (redMaxSize > 700)
                redMaxSize = 700;
            _redMax.sizeDelta = new Vector2(redMaxSize, 24f);

            var yellowMaxSize = (Max - GreenLimitMax) / (Max - Min) * 700;
            yellowMaxSize += yellowMaxSize / 2;
            if (yellowMaxSize > 700)
                yellowMaxSize = 700;
            _yelMax.sizeDelta = new Vector2(yellowMaxSize, 24f);


            if (float.TryParse(Value, out float floatValue))
            {
                if (floatValue < Min)
                    floatValue = Min;

                if (floatValue > Max)
                    floatValue = Max;

                if (floatValue > GreenLimitMin && floatValue < GreenLimitMax)
                    State = SensorState.Green;
                else if (floatValue > YellowLimitMin && floatValue < YellowLimitMax)
                    State = SensorState.Yellow;
                else
                    State = SensorState.Red;

                var position = _indicator.GetComponent<RectTransform>().localPosition;

                position.x = 350 - (Max - floatValue) / (Max - Min) * 700;

                _indicator.GetComponent<RectTransform>().localPosition = position;

                switch (Type)
                {
                    case SensorValueType.Float:
                        _indicator.transform.FindDeepChild("Value").GetComponent<Text>().text = $"{float.Parse(Value):F1}" + " " + Unit;
                        break;

                    case SensorValueType.Int:
                        _indicator.transform.FindDeepChild("Value").GetComponent<Text>().text = $"{float.Parse(Value):F0}" + " " + Unit;
                        break;
                }
            }
        }
    }
}