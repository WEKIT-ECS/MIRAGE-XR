﻿using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Class for defining how a thing should behave
    /// </summary>
    public class ThingBehaviour : MonoBehaviour
    {
        private void OnEnable()
        {
            EventManager.OnShowGuides += ShowGuides;
            EventManager.OnHideGuides += HideGuides;
            EventManager.OnClearAll += Delete;
        }

        private void OnDisable()
        {
            EventManager.OnShowGuides -= ShowGuides;
            EventManager.OnHideGuides -= HideGuides;
            EventManager.OnClearAll -= Delete;
        }

        public void ShowGuides()
        {
            // transform.GetComponentInChildren<PathRoleController>(true).IsVisible = true;
        }

        public void HideGuides()
        {
            // transform.GetComponentInChildren<PathRoleController>(true).IsVisible = false;
        }

        private void Delete()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Show content stored inside this thing.
        /// </summary>
        private void ShowContent()
        {
            foreach (var rend in transform.GetComponentsInChildren<Renderer>())
            {
                rend.enabled = true;
            }

            foreach (var coll in transform.GetComponentsInChildren<Collider>())
            {
                coll.enabled = true;
            }
        }

        /// <summary>
        /// Hide content stored inside this thing.
        /// </summary>
        private void HideContent()
        {
            foreach (var rend in transform.GetComponentsInChildren<Renderer>())
            {
                rend.enabled = false;
            }

            foreach (var coll in transform.GetComponentsInChildren<Collider>())
            {
                coll.enabled = false;
            }
        }

        // private void Update()
        // {
        //     var content = transform.FindDeepChildTag("GuideActive");
        //     if(content == null && !GameObject.Find("UiManager").GetComponent<UiManager>().IsFindActive)
        //         transform.GetComponentInChildren<PathRoleController>(true).IsVisible = false;
        // }
    }
}
