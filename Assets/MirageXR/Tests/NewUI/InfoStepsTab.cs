using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using MirageXR;

public class InfoStepsTab : MonoBehaviour
{

    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private TMP_InputField _inputFieldName;
    [SerializeField] private TMP_InputField _inputFieldDescription;

    [SerializeField] private Action step;

    private void Start()
    {
        _inputFieldName.onValueChanged.AddListener(changeStepName);
        _inputFieldDescription.onValueChanged.AddListener(changeStepDescription);
    }

    public void init(int stepNumber)
    {
        step = activityManager.Activity.actions[stepNumber];

        if (step != null)
        {
            _inputFieldName.text = step.instruction.title;
            _inputFieldDescription.text = step.instruction.description;
        }
    }

    public void changeStepName(string newTitle) {
        step.instruction.title = newTitle;
        EventManager.NotifyOnActionStepTitleChanged();
        EventManager.NotifyActionModified(step);
    }

    public void changeStepDescription(string newDescription)
    {
        step.instruction.description = newDescription;
        EventManager.NotifyOnActionStepDescriptionInputChanged();
        EventManager.NotifyActionModified(step);
    }

}
