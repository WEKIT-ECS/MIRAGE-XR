using LearningExperienceEngine;
using MirageXR;
using i5.Toolkit.Core.VerboseLogging;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class PluginEditor : MonoBehaviour
    {
        [Serializable]
        public struct Plugin
        {
            public Texture2D icon;
            public GameObject pluginPrefab;

            public string id;
            public string name;
            public string manifest;
        }

        [SerializeField] private Transform annotationStartingPoint;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject iconPrefab;
        [SerializeField] private Texture2D defaultIcon;
        [SerializeField] private Plugin[] Plugins;

        private LearningExperienceEngine.Action _action;
        private LearningExperienceEngine.ToggleObject _annotationToEdit;

        public void SetAnnotationStartingPoint(Transform startingPoint)
        {
            annotationStartingPoint = startingPoint;
        }

        public void Close()
        {
            _action = null;
            _annotationToEdit = null;
            gameObject.SetActive(false);
        }

        public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
        {
            gameObject.SetActive(true);
            _action = action;
            _annotationToEdit = annotation;

            GenerateIconList();
        }


        private void OnDisable()
        {
            foreach (var icon in contentContainer.GetComponentsInChildren<RectTransform>())
            {
                if (icon.gameObject != contentContainer.gameObject)
                    Destroy(icon.gameObject);
            }
        }

        private void GenerateIconList()
        {
            foreach (var plugin in Plugins)
            {
                var pluginTex = plugin.icon != null ? plugin.icon : defaultIcon;

                var rect = new Rect(0, 0, pluginTex.width, pluginTex.height);
                var pivot = new Vector2(0.5f, 0.5f);

                Debug.LogDebug("Plugin Name: " + plugin.name);

                var icon = Instantiate(iconPrefab, Vector3.zero, Quaternion.identity);
                icon.transform.FindDeepChild("Image").GetComponent<Image>().sprite = Sprite.Create(pluginTex, rect, pivot);
                icon.transform.FindDeepChild("vfxButton").GetComponent<Button>().onClick.AddListener(() => Create(PluginAsApp(plugin)));
                icon.transform.FindDeepChild("iconLabel").GetComponent<Text>().text = plugin.name;
                icon.transform.SetParent(contentContainer);
                icon.transform.localScale = new Vector3(1, 1, 1);
                icon.transform.localPosition = new Vector3(0, 0, 0);
                icon.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        public void Create(App plugin)
        {
            var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
            if (_annotationToEdit != null)
            {
                LearningExperienceEngine.EventManager.DeactivateObject(_annotationToEdit);
            }
            else
            {
                Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_action.id));
                GameObject originT = GameObject.Find(detectable.id);

                var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                    annotationStartingPoint.transform.rotation,
                    originT.transform.position,
                    originT.transform.rotation);

                _annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(_action, offset);
            }

            _annotationToEdit.predicate = "plugin:" + plugin.name;
            _annotationToEdit.url = plugin.manifest;

            workplaceManager.workplace.apps.Add(plugin);

            _action.appIDs.Add(plugin.id);


            Debug.LogDebug("ACTION ID = " + _annotationToEdit.url);

            LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(_action);
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();

            Close();
        }

        private static App PluginAsApp(Plugin plugin)
        {
            var app = new App
            {
                id = Guid.NewGuid().ToString(),
                name = plugin.name,
                type = "App",
                manifest = plugin.manifest
            };

            return app;
        }
    }
}

