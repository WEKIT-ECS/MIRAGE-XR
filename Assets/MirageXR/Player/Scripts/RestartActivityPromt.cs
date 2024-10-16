using UnityEngine;
using MirageXR;
public class RestartActivityPromt : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.activityManagerOld;

    public async void ResartActivity()
    {
        await activityManager.ActivateFirstAction();
        Destroy(gameObject);
    }

    public void ContinueActivity()
    {
        Destroy(gameObject);
    }
}
