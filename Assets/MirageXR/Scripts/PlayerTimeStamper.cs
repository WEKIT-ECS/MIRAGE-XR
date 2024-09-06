using LearningExperienceEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MirageXR;
using Action = LearningExperienceEngine.Action;

public class PlayerTimeStamper : MonoBehaviour
{

    private string _file;

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnActivityLoadedStamp += ActivityLoadedStamp;
        LearningExperienceEngine.EventManager.OnStepActivatedStamp += StepActivatedStamp;
        LearningExperienceEngine.EventManager.OnStepDeactivatedStamp += StepDeactivatedStamp;
        LearningExperienceEngine.EventManager.OnActivityCompletedStamp += WriteStampFile;
    }

    private void OnDisable()
    {
        LearningExperienceEngine.EventManager.OnActivityLoadedStamp -= ActivityLoadedStamp;
        LearningExperienceEngine.EventManager.OnStepActivatedStamp -= StepActivatedStamp;
        LearningExperienceEngine.EventManager.OnStepDeactivatedStamp -= StepDeactivatedStamp;
        LearningExperienceEngine.EventManager.OnActivityCompletedStamp -= WriteStampFile;
    }

    void Start()
    {
        _file = "";
    }

    void ActivityLoadedStamp(string deviceId, string activityId, string stamp)
    {
        _file = "";
        _file += "Activity " + activityId + " loaded - " + stamp + "\n";
    }

    void StepActivatedStamp(string deviceId, Action activatedAction, string stamp)
    {
        _file += "Step " + activatedAction.id + " activated - " + stamp + "\n";
    }

    void StepDeactivatedStamp(string deviceId, Action deactivatedAction, string stamp)
    {
        _file += "Step " + deactivatedAction.id + " deactivated - " + stamp + "\n";
    }

    void WriteStampFile(string deviceId, string activityId, string stamp)
    {
        _file += "Activity " + activityId + " completed - " + stamp;

        File.WriteAllText(Application.persistentDataPath + "\\ActivityLog-" + DateTime.UtcNow.Year + "-" + DateTime.UtcNow.Month + "-" + DateTime.UtcNow.Day + "-" + DateTime.UtcNow.Hour + "-" + DateTime.UtcNow.Minute + "-" + DateTime.UtcNow.Second + ".txt", _file);
        _file = "";
    }
}
