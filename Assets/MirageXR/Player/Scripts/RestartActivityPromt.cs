using UnityEngine;
using MirageXR;
public class RestartActivityPromt : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

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
