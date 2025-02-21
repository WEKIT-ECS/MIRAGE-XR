using i5.Toolkit.Core.VerboseLogging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DaimonManager : MonoBehaviour
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    public SpeechInputService mySpeechInputMgr { get; private set; }
    private SpeechOutputService mySpeechOutputMgr;

    public GameObject myCharacter { get; set; }
    private Animator myAnimator;

    public GameObject[] lookTargets { get; private set; }

    private Dictionary<string, object> _context = null;
    private bool _waitingForResponse = true;

    public float wait = 0.0f; //currently no wait, may require a fix
    public bool check = false;
    public bool play = false;
    public bool triggerNext = false;
    public bool triggerStep = false;
    public int triggerStepNo = 0;



    // Start is called before the first frame update
    void Start()
    {
        mySpeechInputMgr = GetComponent<SpeechInputService>();
        mySpeechOutputMgr = GetComponent<SpeechOutputService>();
        myAnimator = myCharacter.GetComponent<Animator>();
    }


    IEnumerator Look(float preDelay, float duration, GameObject lookTarget)
    {
        yield return new WaitForSeconds(preDelay);

        Debug.LogTrace("Look=" + "LEFT/RIGHT");
        yield return new WaitForSeconds(duration);

    }


    // Update is called once per frame
    void Update()
    {


        if (check)
        {
            wait -= Time.deltaTime; //reverse count
        }

        if ((wait < 0f) && (check))
        {

            //check that clip is not playing
            check = false;

            if (triggerNext)
            {
                triggerNext = false;
                activityManager.ActivateNextAction();

            }
            if (triggerStep)
            {
                triggerStep = false;
                activityManager.ActivateActionByIndex(triggerStepNo);
            }

            //Now let's start listening again.....
            mySpeechInputMgr.Active = true;
            mySpeechInputMgr.StartRecording();

        }
    }


    // check for exercise name (from ExerciseController.cs)
    // and run the according animation (in myAnimator)
    public void Animate(string exercise)
    {

        switch (exercise)
        {
            case "point":
                myAnimator.Play("Point");
                break;
            case "wave":
                myAnimator.Play("Hello");
                break;
            default:
                break;
        }

    }

}
