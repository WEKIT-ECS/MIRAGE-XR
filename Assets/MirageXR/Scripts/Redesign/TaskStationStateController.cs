using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class TaskStationStateController : MonoBehaviour
    {
        private static LearningExperienceEngine.BrandManager brandManager => LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;

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
            LearningExperienceEngine.EventManager.OnActivateAction += OnActionActivated;
            LearningExperienceEngine.EventManager.OnActionDeleted += OnActionDeleted;
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnActivateAction -= OnActionActivated;
            LearningExperienceEngine.EventManager.OnActionDeleted -= OnActionDeleted;
        }

        private void OnActionActivated(string action)
        {
            UpdateDisplay();
        }

        private void OnActionDeleted(string actionId)
        {
            // TODO: This will destroy all annotations too and any annotations which are created in this step and also exist in other step will be deleted.
            // Find a way to not destroy common annotations
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
                taskStationColor = brandManager.TaskStationColor;
                if (TaskStationDetailMenu.Instance)
                {
                    TaskStationDetailMenu.Instance.ResetTaskStationMenu(this);
                }
            }
            else if (IsNext())
            {
                // this turns the seconddary task station color to default blue;
                // there is a second reference in PathRoleController.cs (line 96),
                // where the original path is set up and switched on/off
                gameObject.SetActive(false);
                taskStationColor = brandManager.NextPathColor;
            }
            else
            {
                gameObject.SetActive(false);
            }

            taskStationRenderer.material.color = taskStationColor;

        }

        public bool IsCurrent()
        {
            return LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActiveActionId.Equals(ActionId);
        }

        private bool IsNext()
        {
            List<LearningExperienceEngine.Action> actions = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActionsOfTypeAction;
            int index = actions.IndexOf(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActiveAction);

            if (index >= actions.Count - 1)
            {
                return false;
            }

            return ActionId.Equals(actions[index + 1].id);
        }
    }
}