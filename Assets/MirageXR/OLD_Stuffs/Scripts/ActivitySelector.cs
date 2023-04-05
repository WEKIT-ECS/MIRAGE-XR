using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{

    public class ActivitySelector : MonoBehaviour
    {
        [SerializeField] private List<string> ActionsList;
        [SerializeField] private Transform ActivityObjectContainer;

        [SerializeField] private Activity Activity;
        [SerializeField] private Workplace Workplace;

        [SerializeField] private Toggle OverwriteToggle;

        [SerializeField] private NonNativeKeyboard InputTextField = null;
        // public TouchScreenKeyboard InputTextField;
        [SerializeField] private Text InputText;
        [SerializeField] private Text MessageText;
        [SerializeField] private GameObject CloseButton;

        [SerializeField] private string Path;

        public struct StoredFile
        {
            public string Path;
            public string Filename;
            public byte[] Bytes;
        }

        [SerializeField]
        private List<String> ExternalActivities = new List<string>();

        private List<String> _handledActivites = new List<String>();

        private List<StoredFile> _storedFiles = new List<StoredFile>();

        public static ActivitySelector Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            // CreateButtons.
            CreateButtons();

            // Close button shouldn't be visible before any activity is loaded.
            CloseButton.SetActive(false);

            InputText.enabled = true;
            WriteMessage("Messages will be loaded here.");
        }

        private void CreateButtons()
        {
            // Empty all buttons.
            foreach (Transform child in ActivityObjectContainer)
            {
                if (child.CompareTag("ActivitySelectionButton"))
                    Destroy(child.gameObject);
            }

            // Get all the files inside the application path and sort them by creation date.
            var sortedFileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles()
                .OrderBy(f => f.LastWriteTime)
                .ToList();

            // Go through all the files...
            foreach (var file in sortedFileInfos)
            {
                // Only interested in the activity json files...
                if (file.Name.ToLower().EndsWith("_activity.json") ||
                    file.Name.ToLower().EndsWith("-activity.json") ||
                    file.Name.ToLower().EndsWith("activity.json"))
                {
                    // Let's give it a try..
                    try
                    {
                        // Read in the presumed activity json.
                        var url = File.ReadAllText(file.FullName);

                        // Convert the file into an activity object.
                        var activity = JsonUtility.FromJson<Activity>(url);

                        // Instantiate a button.
                        var button = Instantiate(Resources.Load<GameObject>("Prefabs/ActivitySelectionPrefab"), ActivityObjectContainer);

                        // Modify button label to match the activity name.
                        button.transform.FindDeepChild("ActivityTitle").GetComponent<Text>().text = activity.name;

                        // Link button name to filename.
                        button.name = file.Name;
                    }
                    catch (Exception e)
                    {
                        EventManager.DebugLog($"Error: Activity Selector: Parsing: Couldn't instantiate activity selection button: {e}");
                    }
                }
            }
        }

        public void ToggleOverwrite()
        {
            EventManager.Click();
            OverwriteToggle.isOn = !OverwriteToggle.isOn;
        }

        // Load activity method called from the load button.
        public void LoadActivity(string url)
        {
            EventManager.Click();

            // Empty out external activities list.
            ExternalActivities.Clear();

            // Empty out the handled activities list.
            _handledActivites.Clear();

            string activityUrl;
            if (string.IsNullOrEmpty(url))
                // Get url from the input field.
                activityUrl = InputTextField.InputField.text;

            else
                activityUrl = url;

            // Extract filename from url.
            var fullname = activityUrl.Split('/');
            var activityFilename = fullname[fullname.Length - 1];

            // If overwrite is not enabled...
            if (!OverwriteToggle.isOn)
            {
                // ...and if a file with the same name exists...
                if (File.Exists(Application.persistentDataPath + "/" + activityFilename))
                {
                    // Write message to input text placeholder.
                    WriteMessage("File already downloaded. Enable overwrite to load it again.");
                }

                // Nothing to overwrite so we can continue normally.
                else
                    StartCoroutine(LoadRoutine(activityUrl));
            }

            // If overwrite is enabled, we can just load all the crap normally.
            else
                StartCoroutine(LoadRoutine(activityUrl));
        }

        // The actual loading routine.
        private IEnumerator LoadRoutine(string activityUrl)
        {
            Debug.Log("Loading activity " + activityUrl);
            // Just to prevent endless loops in file handling...
            _handledActivites.Add(activityUrl);

            // Extract filename from url.
            var fullname = activityUrl.Split('/');
            var activityFilename = fullname[fullname.Length - 1];

            var errorCount = 0;
            WriteMessage("Loading activity file...");

            // Load the activity file from url. Add ticks to prevent any caching issues.
            var activityFile = new WWW(activityUrl + "?" + DateTime.Now.Ticks);
            yield return activityFile;

            // If there are any errors in loading the file...
            if (activityFile.error != null)
            {
                WriteMessage("Couldn't access the activity file. Try again.");
            }

            // File loaded succesfully
            else
            {
                try
                {
                    Debug.Log("Activity file loaded.");
                    // Try to convert to activity json.
                    Activity = JsonUtility.FromJson<Activity>(activityFile.text);

                    // Additional check to see if we really have an activity object.
                    if (!string.IsNullOrEmpty(Activity.id))
                    {
                        // Write activity json to local storage.
                        PrepareForStoring(Application.persistentDataPath + "/", activityFilename, activityFile.bytes);
                        PlayerPrefs.SetString(activityFilename + "-url", activityUrl);
                        PlayerPrefs.Save();
                    }

                    else
                    {
                        WriteMessage("File is not valid activity json.");
                        errorCount++;
                    }
                }
                catch (Exception e)
                {
                    WriteMessage("File is not valid activity json.");
                    errorCount++;
                    Debug.Log(e);
                    throw;
                }

                // If everything good so far...
                if (errorCount.Equals(0))
                {
                    // Load and store activity content.
                    WriteMessage("Loading activity file content...");

                    // Path to local storage.
                    Path = Application.persistentDataPath + "/" + Activity.id + "/";

                    // Create necessary local folder if it doesn't already exist.
                    //if (!Directory.Exists(Path))
                    // Directory.CreateDirectory(Path);


                    // Go through each action...
                    foreach (var action in Activity.actions)
                    {
                        // Enter activate loop...
                        foreach (var activate in action.enter.activates)
                        {
                            // Do magic based on the activation object type...
                            switch (activate.type)
                            {
                                // Handle external action references.
                                case ActionType.Action:
                                case ActionType.Reaction:
                                    // Handle only online files that haven't been handled yet.
                                    if (activate.id.StartsWith("http") && !_handledActivites.Contains(activate.id))
                                    {
                                        // Extract filename from url.
                                        fullname = activate.id.Split('/');
                                        activityFilename = fullname[fullname.Length - 1];

                                        // If overwrite is not enabled...
                                        if (!OverwriteToggle.isOn)
                                        {
                                            // ...start loading only if file is not already existing.
                                            if (!File.Exists(Application.persistentDataPath + "/" + activityFilename))
                                                ExternalActivities.Add(activate.id);
                                        }

                                        // If overwrite is enabled, we can just load all the crap normally.
                                        else
                                            ExternalActivities.Add(activate.id);

                                        Debug.Log("External activity found: " + activate.id);
                                    }
                                    break;

                                // All the other types needs to be checked...
                                default:

                                    // Handle only online files.
                                    if (activate.url.StartsWith("http"))
                                    {
                                        // And if so, start the saving...

                                        // First get the file.
                                        var file = new WWW(activate.url);
                                        yield return file;

                                        // And if no errors in getting the file...
                                        if (file.error == null)
                                        {
                                            try
                                            {
                                                // Split the url...
                                                var url = activate.url.Split('/');

                                                // Just to get the filename.
                                                var filename = url[url.Length - 1];

                                                // Prepare file for storing...
                                                PrepareForStoring(Path, filename, file.bytes);
                                            }
                                            catch (Exception e)
                                            {
                                                WriteMessage("Couldn't load a file in the activity.");
                                                errorCount++;
                                                Debug.Log(e);
                                                throw;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        // Let's go through the exit loop just to get the activity references...
                        foreach (var activate in action.exit.activates)
                        {
                            // Do magic based on the activation object type...
                            switch (activate.type)
                            {
                                // Handle external action references.
                                case ActionType.Action:
                                case ActionType.Reaction:
                                    // Handle only online files that haven't been handled yet.
                                    if (activate.id.StartsWith("http") && !_handledActivites.Contains(activate.id))
                                    {
                                        // Extract filename from url.
                                        fullname = activate.id.Split('/');
                                        activityFilename = fullname[fullname.Length - 1];

                                        // If overwrite is not enabled...
                                        if (!OverwriteToggle.isOn)
                                        {
                                            // ...start loading only if file is not already existing.
                                            if (!File.Exists(Application.persistentDataPath + "/" + activityFilename))
                                                ExternalActivities.Add(activate.id);
                                        }

                                        // If overwrite is enabled, we can just load all the crap normally.
                                        else
                                            ExternalActivities.Add(activate.id);

                                        Debug.Log("External activity found: " + activate.id);
                                    }
                                    break;
                            }
                        }
                    }
                }

                // If everything still ok...
                if (errorCount.Equals(0))
                {
                    // If activity workplace model url starts with http...
                    if (Activity.workplace.StartsWith("http"))
                    {
                        WriteMessage("Loading workplace file...");
                        Debug.Log("Workplace file found: " + Activity.workplace);

                        // Load the workplace file from url. Add ticks to prevent any caching issues.
                        var workplaceFile = new WWW(Activity.workplace + "?" + DateTime.Now.Ticks);
                        yield return workplaceFile;

                        // If there are any errors in loading the file...
                        if (workplaceFile.error != null)
                        {
                            WriteMessage("Couldn't access the workplace file. Try again.");
                        }

                        // File loaded succesfully
                        else
                        {
                            try
                            {
                                Debug.Log("Workplace file loaded.");
                                // Try to convert to activity json.
                                Workplace = JsonUtility.FromJson<Workplace>(workplaceFile.text);

                                // Additional check to see if we really have an activity object.
                                if (!string.IsNullOrEmpty(Workplace.id))
                                {
                                    // Prepare workplace file for storing.
                                    var url = Activity.workplace.Split('/');
                                    var filename = url[url.Length - 1];

                                    PrepareForStoring(Application.persistentDataPath + "/", filename, workplaceFile.bytes);
                                }

                                else
                                {
                                    WriteMessage("Workplace json is not valid.");
                                    Debug.Log("Not a valid workplace file!");
                                    errorCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                WriteMessage("Workplace json is not valid.");
                                errorCount++;
                                Debug.Log(e);
                                throw;
                            }

                            // If even still no errors...
                            if (errorCount.Equals(0))
                            {
                                WriteMessage("Loading workplace file detectables...");

                                // Load all the downloadable content needed for detectables.
                                foreach (var detectable in Workplace.detectables)
                                {
                                    // Path to local storage.
                                    var path = System.IO.Path.Combine(Application.persistentDataPath, Workplace.id, "detectables", detectable.id);

                                    // Load content needed for Vuforia image targets.
                                    switch (detectable.type)
                                    {
                                        case "image":
                                        {
                                            // Fetch the image target .xml and .dat files from url folder.
                                            var xmlFile = new WWW(detectable.url + detectable.id + ".xml" + "?" +
                                                                  DateTime.Now.Ticks);
                                            yield return xmlFile;

                                            var datFile = new WWW(detectable.url + detectable.id + ".dat" + "?" +
                                                                  DateTime.Now.Ticks);
                                            yield return datFile;

                                            try
                                            {
                                                // Check that we have the .xml file.
                                                if (xmlFile.error != null)
                                                {
                                                    throw new FileLoadException(detectable.id +
                                                        ": couldn't get the .xml file.");
                                                }

                                                // Check that we have the .dat file.
                                                if (datFile.error != null)
                                                {
                                                    throw new FileLoadException(detectable.id +
                                                        ": couldn't get the .dat file.");
                                                }

                                                // Create necessary local folder if it doesn't already exist.
                                                // if (!Directory.Exists(path))
                                                // Directory.CreateDirectory(path);

                                                // Prepare .xml file for storing.
                                                PrepareForStoring(path, detectable.id + ".xml", xmlFile.bytes);

                                                // Write .dat file to local folder.
                                                PrepareForStoring(path, detectable.id + ".dat", datFile.bytes);
                                            }
                                            catch (Exception e)
                                            {
                                                errorCount++;
                                                EventManager.DebugLog(
                                                    "Invalid detectable in workplace json: " + detectable.id);
                                                Debug.Log(e);
                                                throw;
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Local workplace file in resources.
                    else if (Activity.workplace.StartsWith("resources://"))
                    {
                        var asset = Resources.Load(Activity.workplace.Replace("resources://", "")) as TextAsset;
                        yield return new WaitForSeconds(0.1f);

                        // Create workplace file object from json
                        try
                        {
                            // For loading from resources
                            Workplace = JsonUtility.FromJson<Workplace>(asset.text);

                            // Additional check to see if we really have an activity object.
                            if (string.IsNullOrEmpty(Workplace.id))
                            {
                                errorCount++;
                                WriteMessage("Workplace json not valid.");
                            }
                        }
                        catch (System.Exception e)
                        {
                            WriteMessage("Couldn't access the workplace file. Try again.");
                            Debug.Log(e);
                            throw;
                        }
                    }

                    // Local file in persistent data path.
                    else
                    {
                        var url = File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, $"{Activity.workplace}.json"));

                        try
                        {
                            Workplace = JsonUtility.FromJson<Workplace>(url);

                            // Additional check to see if we really have an activity object.
                            if (string.IsNullOrEmpty(Workplace.id))
                            {
                                errorCount++;
                                WriteMessage("Workplace json not valid.");
                            }
                        }
                        catch (Exception e)
                        {
                            WriteMessage("Couldn't access the workplace file. Try again.");
                            Debug.Log(e);
                            throw;
                        }
                    }
                }

                // Final check. If all is good, store files.
                if (errorCount.Equals(0))
                    StoreFiles();
            }
        }

        // Prepare for storing.
        private void PrepareForStoring(string path, string filename, byte[] bytes)
        {
            Debug.Log("Preparing for storing: " + path + "/" + filename);
            var store = new StoredFile
            {
                Path = path,
                Filename = filename,
                Bytes = bytes
            };
            _storedFiles.Add(store);
        }

        // Store files.
        private void StoreFiles()
        {
            WriteMessage("Storing files to local filesystem...");
            try
            {
                foreach (var file in _storedFiles)
                {
                    if (!Directory.Exists(file.Path))
                        Directory.CreateDirectory(file.Path);

                    File.WriteAllBytes(file.Path + file.Filename, file.Bytes);
                    Debug.Log("Stored a file " + file.Filename);
                }
            }
            catch (Exception e)
            {
                WriteMessage("Some problems in stroring files. Try again...");
                Debug.Log(e);
                throw;
            }

            HandleNext();
        }

        private void HandleNext()
        {
            Debug.Log("Handle next.");
            _storedFiles.Clear();
            if (ExternalActivities.Count != 0)
            {
                StartCoroutine(LoadRoutine(ExternalActivities[0]));
                ExternalActivities.RemoveAt(0);
            }

            else
            {
                WriteMessage("Activity loaded succesfully!");
                CreateButtons();
            }
        }

        private void WriteMessage(string message)
        {
            //InputText.enabled = false;
            MessageText.enabled = true;
            MessageText.text = message;
        }
    }
}