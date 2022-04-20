using System.Collections;
using UnityEngine;
using MirageXR;

/// <summary>
/// Just used for MirageXR player testing.
/// </summary>
public class DummyLogic : MonoBehaviour
{
    private const string _activityURL = "resources://altec_activity";

    [SerializeField] private GameObject SensorState;

    private void OnEnable ()
    {
        EventManager.OnPlayerReset += PlayerReset;
    }

    private void OnDisable ()
    {
        EventManager.OnPlayerReset -= PlayerReset;
    }

    private void PlayerReset ()
    {
        //SensorState.SetActive (false);
    }

    /// <summary>
    /// Load activity file.
    /// </summary>
    private void LoadActivity ()
    {
        // First load. Uses the default activity.
        if (string.IsNullOrEmpty (PlayerPrefs.GetString ("activityUrl")))
        {
            // Save to persistent storage.
            PlayerPrefs.SetString ("activityUrl", _activityURL);
            PlayerPrefs.Save ();
        }

        // Get the activity url from persistent storage.
        var activity = PlayerPrefs.GetString ("activityUrl");

        // For VTT demos that are using the IoT functionality.
        switch (activity)
        {
            // Enable the sensor state display object for VTT demos.
            case "resources://vttdemoactivity":
            case "http://192.168.0.1/activities/new_activity.json":
                SensorState.SetActive (true);
                break;
            
            // Disable the sensor state object for all the other activities.
            default:
                SensorState.SetActive (false);
                break;
        }

        EventManager.ParseActivity (PlayerPrefs.GetString ("activityUrl"));
    }

    // Use this for initialization
    private void Start ()
    {
        EventManager.ParseActivity("http://192.168.0.1/activities/AltecTrialActivity.json");

        //LoadActivity ();

        //EventManager.ParseActivity ("resources://scenario0_activity");

        //EventManager.ParseActivity ("resources://vttdemoactivity");

        // Start the process by loading an activity file from server...
        //EventManager.ParseActivity ("https://dl.dropboxusercontent.com/s/vuw23fi2v88wj33/altec_activity.json");

        // ...or from the inbuilt resources folder.
        //EventManager.ParseActivity ("resources://ebit_activity");
        //EventManager.ParseActivity ("resources://altec_activity");
        //EventManager.ParseActivity( "https://www.dropbox.com/s/lmhsb15m262xfkx/altec_activity.json?dl=0" );
        //EventManager.ParseActivity ("http://192.168.0.1/activities/new_activity.json");


        //EventManager.ParseActivity ("resources://lt_activity");
        //EventManager.ParseActivity ("https://dl.dropboxusercontent.com/s/l0pgmkfht9j5y23/lt_activity.json");
    }

    /// <summary>
    /// Load VTT Demo activity.
    /// </summary>
    public void VttDemoMode ()
    {
        // Load the VTT Demo trial activity.
        PlayerPrefs.SetString ("activityUrl", "resources://vttdemoactivity");
        PlayerPrefs.Save ();
        Maggie.Speak ("Loading VTT Demo Activity");
        EventManager.PlayerReset ();
    }

    /// <summary>
    /// Load Lufttransport trial activity.
    /// </summary>
    public void LuftTrialMode ()
    {
        // Load the Lufttransport trial activity.
        PlayerPrefs.SetString ("activityUrl", "resources://lt_activity");
        PlayerPrefs.Save ();
        Maggie.Speak ("Loading Lufttransport Trial Activity");
        EventManager.PlayerReset ();
    }

    /// <summary>
    /// Load ALTEC trial activity.
    /// </summary>
    public void AltecTrialMode ()
    {
        // Load the ALTEC trial activity.
        PlayerPrefs.SetString ("activityUrl", "resources://altec_activity");
        PlayerPrefs.Save ();
        Maggie.Speak ("Loading ALTEC Trial Activity");
        EventManager.PlayerReset ();
    }

    /// <summary>
    /// Load Ebit trial activity.
    /// </summary>
    public void EbitTrialMode ()
    {
        // Load the Ebit trial activity.
        PlayerPrefs.SetString ("activityUrl", "resources://ebit_activity");
        PlayerPrefs.Save ();
        Maggie.Speak ("Loading Ebit Trial Activity");
        EventManager.PlayerReset ();
    }

    /// <summary>
    /// Load ESA Demo activity.
    /// </summary>
    public void EsaDemoActivity ()
    {
        // Load the ESA Demo activity.
        PlayerPrefs.SetString ("activityUrl", "http://192.168.0.1/activities/new_activity.json");
        PlayerPrefs.Save ();
        Maggie.Speak ("Loading ESA Demo Activity");
        EventManager.PlayerReset ();
    }

    /// <summary>
    /// Load UI trial activity.
    /// </summary>
    public void UiTrialActivity()
    {
        // Load the ESA Demo activity.
        PlayerPrefs.SetString("activityUrl", "https://dropboxusercontent.com/s/hp0hnwq7p1nxz4y/wekitTrialActivity.json");
        PlayerPrefs.Save();
        Maggie.Speak("Loading ESA Demo Activity");
        EventManager.PlayerReset();
    }
}
