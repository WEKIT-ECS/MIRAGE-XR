using MirageXR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathRoleController : MonoBehaviour
{
    private static LearningExperienceEngine.BrandManager brandManager => LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [Header("Elements")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TextMesh[] captionTexts;

    [Header("Icons")]
    [SerializeField] private Sprite homeIcon;
    [SerializeField] private Sprite settingsIcon;
    [SerializeField] private Sprite taskStationIcon;

    [SerializeField] private PathRole role;

    private string actionId;

    private PathSegmentsController segmentsController;

    private string ActionId
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

    public PathRole Role
    {
        get => role;
        set
        {
            role = value;
            UpdateDisplay();
        }
    }

    private void Awake()
    {
        segmentsController = GetComponent<PathSegmentsController>();
        LearningExperienceEngine.EventManager.OnActivateAction += OnActionActivated;
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnActivateAction -= OnActionActivated;
    }

    private void UpdateDisplay()
    {
        Color pathColor;
        string caption;
        Sprite icon;

        switch (role)
        {
            case PathRole.HOME:
                segmentsController.endOffset = 0f;
                pathColor = brandManager.UIPathColor;
                caption = "Home";
                icon = homeIcon;
                break;
            case PathRole.SETTINGS:
                segmentsController.endOffset = 0f;
                pathColor = brandManager.UIPathColor;
                caption = "Settings";
                icon = settingsIcon;
                break;
            case PathRole.TASKSTATION:
                segmentsController.endOffset = 0.07f;
                int taskStationIndex = GetTaskStationIndex();
                string positionCaption;
                if (IsCurrent())
                {
                    gameObject.SetActive(false);
                    positionCaption = "Current";
                    pathColor = brandManager.TaskStationColor;
                }
                else if (IsNext())
                {
                    // if this is switched to true, the next task station will
                    // be displayed in turquoise; needs to be complemented
                    // with update of line 72 set to true in TaskStationStateController.cs
                    gameObject.SetActive(false);
                    positionCaption = "Next";
                    pathColor = brandManager.NextPathColor;
                }
                else
                {
                    gameObject.SetActive(false);
                    positionCaption = string.Empty;
                    pathColor = Color.blue;
                }

                // show the index; add 1 so that the displayed index starts at 1
                caption = $"{taskStationIndex + 1:00}\n{positionCaption}";
                icon = taskStationIcon;

                // After the new design (moving the annotation menu to taskstaion) we remove the caption on current diamond (Abbas)
                if (IsCurrent())
                {
                    caption = string.Empty;
                }

                break;
            default:
                gameObject.SetActive(false);
                icon = null;
                caption = string.Empty;
                pathColor = Color.cyan;
                break;
        }

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.material.color = pathColor;
            }
        }

        iconRenderer.color = pathColor;
        iconRenderer.sprite = icon;
        foreach (TextMesh captionText in captionTexts)
        {
            captionText.text = caption;
        }
    }

    private int GetTaskStationIndex()
    {
        var actions = activityManager.ActionsOfTypeAction;

        var index = actions.IndexOf(actions.FirstOrDefault(p => p.id.Equals(ActionId)));
        return index;
    }

    private bool IsCurrent()
    {
        if (string.IsNullOrEmpty(activityManager.ActiveActionId))
        {
            return false;
        }
        return activityManager.ActiveActionId.Equals(ActionId);
    }

    private bool IsNext()
    {
        List<LearningExperienceEngine.Action> actions = activityManager.ActionsOfTypeAction;

        int index = actions.IndexOf(activityManager.ActiveAction);
        if (index >= actions.Count - 1)
        {
            return false;
        }
        return ActionId.Equals(actions[index + 1].id);
    }

    private void OnActionActivated(string action)
    {
        UpdateDisplay();
    }
}

public enum PathRole
{
    HOME, SETTINGS, TASKSTATION
}
