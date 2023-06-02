using MirageXR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    [SerializeField] private GameObject stepPrefab;
    [SerializeField] private Color completedColor;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color uncompletedColor;

    private List<Image> stepInstances = new List<Image>();

    private void OnEnable()
    {
        EventManager.OnInitUi += UpdateUI;
        EventManager.OnActivateAction += OnActivateAction;
    }

    private void OnDisable()
    {
        EventManager.OnInitUi -= UpdateUI;
        EventManager.OnActivateAction -= OnActivateAction;
    }

    private void OnActivateAction(string actionId)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        int numberOfActions = activityManager.ActionsOfTypeAction.Count;

        for (int i = 0; i < numberOfActions; i++)
        {
            if (i < stepInstances.Count)
            {
                stepInstances[i].gameObject.SetActive(true);
            }
            else
            {
                GameObject stepInstance = Instantiate(stepPrefab, transform);
                Image img = stepInstance.GetComponent<Image>();
                stepInstances.Add(img);
            }

            List<Action> actions = activityManager.ActionsOfTypeAction;

            if (actions[i].id == activityManager.ActiveActionId)
            {
                stepInstances[i].color = BrandManager.Instance.SecondaryColor;
            }
            else if (actions[i].isCompleted)
            {
                stepInstances[i].color = completedColor;
            }
            else
            {
                stepInstances[i].color = uncompletedColor;
            }
        }

        for (int i = numberOfActions; i < stepInstances.Count; i++)
        {
            stepInstances[i].gameObject.SetActive(false);
        }
    }
}
