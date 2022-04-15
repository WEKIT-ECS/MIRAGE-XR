using Microsoft.MixedReality.Toolkit.UI;
using MirageXR;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MirageXR
{
    public class TaskStationStateController : MonoBehaviour
    {
        [SerializeField] private Renderer taskStationRenderer;

        private string actionId;

        public string ActionId
        {
            get
            {
                if (string.IsNullOrEmpty(actionId))
                {
                    actionId = transform.parent.parent.name;
                }
                return actionId;
            }
        }

        private void Start()
        {

            EventManager.OnActivateAction += OnActionActivated;
            EventManager.OnActionDeleted += OnActionDeleted;
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            EventManager.OnActivateAction -= OnActionActivated;
            EventManager.OnActionDeleted -= OnActionDeleted;
        }

        private void OnActionActivated(string action)
        {
            UpdateDisplay();
        }

        private void OnActionDeleted(string actionId)
        {
            //TODO: This will destroy all annotations too and any annotations which are created in this step and also exist in other step will be deleted. 
            //Find a way to not destroy common annotations
            if (ActionId == actionId)
            {
                Destroy(transform.parent.parent.gameObject);
            }
        }

        private void UpdateDisplay()
        {
            Color taskStationColor = Color.red;

            if (IsCurrent())
            {
                gameObject.SetActive(true);
                taskStationColor = BrandManager.Instance.GetTaskStationColor();
                TaskStationDetailMenu.Instance.ResetTaskStationMenu(this);
            }
            else if (IsNext())
            {
                gameObject.SetActive(true);
                taskStationColor = BrandManager.Instance.GetNextPathColor();
            }
            else
            {
                gameObject.SetActive(false);

            }

            taskStationRenderer.material.color = taskStationColor;

        }



        public bool IsCurrent()
        {          
            return ActivityManager.Instance.ActiveActionId.Equals(ActionId);
        }

        private bool IsNext()
        {
            List<Action> actions = ActivityManager.Instance.ActionsOfTypeAction;
            
            int index = actions.IndexOf(ActivityManager.Instance.ActiveAction);

            if (index >= actions.Count - 1)
            {
                return false;
            }

            
            return ActionId.Equals(actions[index + 1].id);
        }
    }
}