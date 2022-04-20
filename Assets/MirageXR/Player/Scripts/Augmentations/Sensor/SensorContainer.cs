using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{

    public class SensorContainer : MonoBehaviour
    {

        private Text _title;
        public string Title { get; set; }

        public Transform Container { get; private set; }

        [SerializeField] private GameObject Button;

        private void OnEnable()
        {
            EventManager.OnClearAll += Delete;
            EventManager.OnShowSensors += ShowContent;
            EventManager.OnHideSensors += HideContent;
        }

        private void OnDisable()
        {
            EventManager.OnClearAll -= Delete;
            EventManager.OnShowSensors -= ShowContent;
            EventManager.OnHideSensors -= HideContent;
        }

        // Use this for initialization
        void Awake()
        {
            _title = transform.FindDeepChild("TitleText").GetComponent<Text>();
            Container = transform.FindDeepChild("Container").transform;

            HideContent();
        }

        public void ShowContent()
        {
            Button.SetActive(false);
            Container.localScale = Vector3.one;
        }

        public void HideContent()
        {
            Button.SetActive(true);
            Container.localScale = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            _title.text = Title;
        }

        private void Delete()
        {
            Destroy(gameObject);
        }
    }
}