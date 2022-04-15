using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class OldTutorialManager : MonoBehaviour
    {
        public static OldTutorialManager Instance;

        [SerializeField] private GameObject arrowPrefab;

        private bool tutorialIsDone = false;
        private GameObject arrow;
        private Text txt;
        private Steps currentStep = 0;
        private List<Tutorial> tutorials = new List<Tutorial>();
        private Tutorial currentTutorialStep;
        private float arrowOffset = -0.01f;
        [HideInInspector] public Button tutorialBtn;
        [HideInInspector] public bool homeButtonPressed;

        public Steps CurrentStep()
        {
            return currentStep;
        }

        public enum Steps
        {
            ToggleTutorial,
            UnlockWindow,
            DragWindow,
            CreateActivity,
            MoveTaskStation,
            ChangeActivityName,
            CreateAction,
            CreateAnnotation,
            AddAnnotiation,
            WriteLabel,
            AcceptAnotation,
            ChangeActionName,
            AddDescription,
            DeleteAction,
            SaveActivity,
            UploadActivity,
            ReturnToActivities,
            LoadActivity
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void Start()
        {
            //Check if tutorial is done before
            tutorialIsDone = PlayerPrefs.GetInt("TutorialStatus") == 1;

            if (!PlatformManager.Instance.WorldSpaceUi)
            {
                return;
            }
            DontDestroyOnLoad(gameObject);
            Invoke(nameof(Init), 0.2f);
        }

        /// <summary>
        /// Initiate the tutorial after 0.5 sec
        /// </summary>
        private async void Init()
        {

            if (tutorialIsDone)
            {
                CloseTutorial();
                return;
            }

            OpenTutorial();

            await MoodleManager.Instance.GetArlemList();
            StartCoroutine(AddTutorialCancellationToActivityButtons());
        }

        /// <summary>
        /// Add to all acitivity buttons to check if any activity get opened, the tutorial should be closed
        /// </summary>
        /// <returns></returns>
        private IEnumerator AddTutorialCancellationToActivityButtons()
        {
            bool hololens = PlatformManager.Instance.WorldSpaceUi;
            var activityList = hololens ? GameObject.Find("ActivitySelectionMenu") : GameObject.Find("ActivitySelectionMenuNonHeadset");
            var activityButtons = activityList.transform.FindDeepChild("ActivityList").GetComponentsInChildren<Button>();

            //Turn the tutorial off if the user load an activity
            foreach (var btn in activityButtons)
            {
                btn.onClick.AddListener(() => {
                    if (currentStep != Steps.LoadActivity)
                    {
                        CloseTutorial();
                    }
                });
            }

            yield return null;
        }

        /// <summary>
        /// Move the guild arrow to this position
        /// </summary>
        /// <param name="pos"></param>
        public void MoveArrowTo(GameObject obj, string str)
        {
            try
            {
                if (!tutorialIsDone && arrow)
                {
                    txt = arrow.GetComponent<TutorialArrow>().GetInstructionText();
                    arrow.SetActive(true);
                    arrow.transform.position = obj.transform.position + Vector3.forward * arrowOffset;
                    arrow.transform.rotation = obj.transform.rotation;
                    txt.text = str;
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Tutorial is done and will not be shoen next time
        /// </summary>
        public void CloseTutorial()
        {
            tutorialIsDone = true;
            homeButtonPressed = false;
            PlayerPrefs.SetInt("TutorialStatus", 1); //Done
            if (arrow) Destroy(arrow);

            for (int i = 0; i < tutorials.Count; i++)
            {
                tutorials[i] = null;
            }

            tutorials.Clear();

            //Adjust tutorial button
            if (tutorialBtn)
            {
                tutorialBtn.onClick.RemoveListener(CloseTutorial);
                tutorialBtn.onClick.AddListener(OpenTutorial);
                tutorialBtn.GetComponent<TutorialButton>().ResetButton();
            }

            Debug.Log("Tutorisl is closed");
        }

        /// <summary>
        /// Check if tutorial is already done
        /// </summary>
        /// <returns></returns>
        public bool TutorialIsDone()
        {
            return tutorialIsDone;
        }

        /// <summary>
        /// Reset tutorial
        /// </summary>
        public void OpenTutorial()
        {
            tutorialIsDone = false;
            currentStep = Steps.UnlockWindow; //first step
            PlayerPrefs.SetInt("TutorialStatus", 0); //reset

            if (!arrow)
                arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);

            //Adjust tutorial button
            if (tutorialBtn)
            {
                tutorialBtn.onClick.RemoveListener(OpenTutorial);
                tutorialBtn.onClick.AddListener(CloseTutorial);
            }

            Debug.Log("Tutorisl is started");
        }

        /// <summary>
        /// change the activation state of tutorial 
        /// </summary>
        public void ToggleTutorial()
        {
            if (tutorialIsDone)
            {
                OpenTutorial();
            }
            else
            {
                CloseTutorial();
            }
        }

        /// <summary>
        /// Jump to the next step of the tutorial
        /// </summary>
        public void NextStep()
        {
            int currentIndex = (int)currentStep;
            if (currentIndex < Enum.GetNames(typeof(Steps)).Length)
            {
                int nextIndex = (int)currentStep + 1;
                currentStep = (Steps)nextIndex;
            }
            else
            {
                CloseTutorial();
            }

            CheckTutorialNeeds();
            //print((int)currentStep);
        }


        /// <summary>
        /// Jump to the previous step of the tutorial
        /// </summary>
        public void PrevStep()
        {
            int currentIndex = (int)currentStep;
            if (currentIndex > 0)
            {
                int prevIndex = (int)currentStep - 1;
                currentStep = (Steps)prevIndex;
            }
            else
            {
                OpenTutorial();
            }

            CheckTutorialNeeds();
            //print((int)currentStep);
        }


        /// <summary>
        /// Reset eachstep by default texts and menus active/deavtive
        /// </summary>
        private async void CheckTutorialNeeds()
        {
            GameObject ActivitySelectorMenu;
            GameObject actionDetailView;
            //GameObject actionList;

            bool Hololens = PlatformManager.Instance.WorldSpaceUi;
            ActivitySelectorMenu = (Hololens ? GameObject.Find("ActivitySelectionMenu") : GameObject.Find("ActivitySelectionMenuNonHeadset"));
            actionDetailView = (Hololens ? GameObject.Find("ActionDetailView") : GameObject.Find("ActionDetailViewNonHeadset"));
            //actionList = (Hololens ? GameObject.Find("ActionList") : GameObject.Find("ActionListNonHeadset"));

            switch (currentStep)
            {
                case Steps.DragWindow:
                    ActivitySelectorMenu.transform.Find("Lock").GetComponent<Button>().onClick.Invoke();
                    break;
                case Steps.CreateActivity:
                    await ServiceManager.GetService<EditorSceneService>().UnloadExistingScene();
                    EventManager.ShowActivitySelectionMenu();
                    break;
                case Steps.MoveTaskStation:
                    if (ActivitySelectorMenu)
                        ActivitySelectorMenu.transform.FindDeepChild("ButtonAdd").GetComponent<Button>().onClick.Invoke();
                    break;
                case Steps.ChangeActivityName:
                    GameObject ActivityTitle;
                    if ((ActivityTitle = GameObject.Find("TitleInputField")) != null)
                        ActivityTitle.GetComponent<InputField>().text = "New Activity";
                    break;
                case Steps.CreateAction:
                    if (GameObject.Find("TitleInputField").GetComponent<InputField>().text.Contains("New Activity")) //if user did not changed
                        GameObject.Find("TitleInputField").GetComponent<InputField>().text = "Activity Nr1";
                    break;
                case Steps.AddAnnotiation:
                    if (actionDetailView)
                        GameObject.Find("MenuPanel").transform.Find("AddButton").GetComponent<Button>().onClick.Invoke();
                    GameObject TextEditor = null;
                    if ((TextEditor = actionDetailView.transform.FindDeepChild("TextEditor").gameObject) != null)
                        TextEditor.SetActive(false);
                    break;
                case Steps.WriteLabel:
                    if (GameObject.Find("AnnotationAddMenu") && !actionDetailView.transform.FindDeepChild("TextEditor").gameObject.activeInHierarchy)
                        GameObject.Find("AnnotationAddMenu").transform.GetChild(4).GetComponent<Button>().onClick.Invoke();
                    else
                    {
                        //reset label
                        var textInputField = GameObject.Find("TextEditor").transform.Find("InputField").gameObject;
                        textInputField.GetComponent<InputField>().text = "";
                    }
                    break;
                case Steps.AcceptAnotation:
                    TextEditor = null;
                    if ((TextEditor = actionDetailView.transform.FindDeepChild("TextEditor").gameObject) != null) //if text editor is closed open it
                        TextEditor.SetActive(true);
                    GameObject.Find("TextEditor").transform.Find("InputField").GetComponent<InputField>().text = "The label is empty"; //if empty add sample label
                    TextEditor.transform.Find("AcceptButton").gameObject.SetActive(true); //show accept button
                    break;
                case Steps.ChangeActionName:
                    InputField actionName = null;
                    if ((actionName = GameObject.Find("ActionTitleInputField").GetComponent<InputField>()) != null)
                        actionName.text = "Action Step";
                    try
                    {
                        actionDetailView.transform.FindDeepChild("AcceptButton").GetComponent<Button>().onClick.Invoke();
                    }
                    catch { }

                    break;
                case Steps.AddDescription:
                    if (GameObject.Find("DescriptionInputField").GetComponent<InputField>().text.Contains("Action Step")) //if user did not changed
                        GameObject.Find("DescriptionInputField").GetComponent<InputField>().text = "Step Nr1";
                    var descriptionBtn = GameObject.Find("DescriptionInputField");
                    descriptionBtn.GetComponent<InputField>().text = "Add task step description here";
                    break;
                case Steps.UploadActivity:
                    homeButtonPressed = false;
                    break;
                case Steps.LoadActivity:
                    await ServiceManager.GetService<EditorSceneService>().UnloadExistingScene();
                    EventManager.ShowActivitySelectionMenu();
                    break;
                default:
                    break;
            }

        }


        private void Update()
        {

            //if (!tutorialIsDone)
            //{
            //    currentTutorialStep = tutorials.Find(x => x.ID == (int)currentStep);

            //    //create this tutorial if not exists
            //    if (currentTutorialStep == null)
            //    {
            //        currentTutorialStep = new Tutorial((int)currentStep);
            //        tutorials.Add(currentTutorialStep);
            //    }

            //    UpdateTutorials();
            //}

        }


        /// <summary>
        /// Call when the current step is done and will start the next step
        /// </summary>
        /// <param name="nextStep"></param>
        private void TutorialButtonIsClicked(Steps nextStep)
        {
            currentTutorialStep.Done = true;
            currentStep = nextStep;
        }

        /// <summary>
        /// Update the tutorials steps
        /// </summary>
        private async void UpdateTutorials()
        {
            var activitySelectorMenu = GameObject.Find("ActivitySelectionMenu");
            var actionList = GameObject.Find("ActionList");
            var actionDetailView = GameObject.Find("ActionEditorView");

            bool activitySelectorMenuIsOpen = activitySelectorMenu != null;
            bool actionListIsOpen = actionList != null;
            bool actionDetailViewIsOpen = actionDetailView != null;

            GameObject tutorialObj;

            switch (currentStep)
            {
                //each case is a tutorial step
                case Steps.ToggleTutorial:

                    if (tutorialBtn && !currentTutorialStep.Done)
                    {
                        var msg = "Tap this button to close and open the tutorial.";
                        MoveArrowTo(tutorialBtn.gameObject, msg);
                        tutorialBtn.onClick.AddListener(() => TutorialButtonIsClicked(Steps.UnlockWindow));
                    }
                    break;
                case Steps.UnlockWindow:
                    if (activitySelectorMenuIsOpen && !currentTutorialStep.Done)
                    {
                        if (!PlatformManager.Instance.WorldSpaceUi)
                            TutorialButtonIsClicked(Steps.CreateActivity); //jump to CreateActivity if it is not hololens

                        var lockBtn = GameObject.Find("Lock");

                        var msg = "Tap this button to unlock the window.";
                        MoveArrowTo(lockBtn, msg);
                        lockBtn.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.DragWindow); });
                        lockBtn.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.DragWindow); });
                    }
                    break;
                case Steps.DragWindow:
                    if (activitySelectorMenuIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("Header");
                        if (!tutorialObj) break;
                        var msg = "Now you can move the window by pinching the header and drag.";
                        MoveArrowTo(tutorialObj, msg);

                        //here should add an if statment for non headset devices
                        tutorialObj.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { TutorialButtonIsClicked(Steps.CreateActivity); });
                    }
                    break;
                case Steps.CreateActivity:
                    if (activitySelectorMenuIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("ButtonAdd");
                        if (!tutorialObj) break;
                        var msg = "Tap this button to create a new activity.";
                        MoveArrowTo(tutorialObj, msg);
                        tutorialObj.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.MoveTaskStation); });
                        tutorialObj.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.MoveTaskStation); });
                    }
                    break;
                case Steps.MoveTaskStation:
                    if (GameObject.Find("PlayerTaskStation(Clone)") && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("PlayerTaskStation(Clone)");
                        if (!tutorialObj) break;
                        var msg = "Move the diamond by pinching the header and drag.";
                        MoveArrowTo(tutorialObj, msg);

                        //here should add an if statment for non headset devices
                        tutorialObj.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { TutorialButtonIsClicked(Steps.ChangeActivityName); });
                    }
                    break;
                case Steps.ChangeActivityName:
                    if (actionListIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("TitleInputField");
                        if (!tutorialObj) break;
                        var msg = "Tap this button to rename the activity.";
                        MoveArrowTo(tutorialObj, msg);

                        var txt = tutorialObj.GetComponent<InputField>().text;
                        if (txt != "" && !txt.Contains("New Activity"))
                        {
                            TutorialButtonIsClicked(Steps.CreateAction);
                        }
                    }
                    break;
                case Steps.CreateAction:
                    if (actionListIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("ButtonAdd");
                        if (!tutorialObj) break;
                        var msg = "Tap this button to create a new action";
                        MoveArrowTo(tutorialObj, msg);
                        tutorialObj.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.CreateAnnotation); });
                        tutorialObj.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.CreateAnnotation); });
                    }
                    break;
                case Steps.CreateAnnotation:
                    if (actionDetailViewIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = await GetTargetObj("AddButton");
                        if (!tutorialObj) break;
                        var msg = "Tap this button to create a new annotaion.";
                        MoveArrowTo(tutorialObj, msg);
                        tutorialObj.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.AddAnnotiation); });
                        tutorialObj.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.AddAnnotiation); });
                    }
                    break;
                case Steps.AddAnnotiation:
                    if (actionDetailViewIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("AnnotationAddMenu");
                        if (!tutorialObj) break;
                        var msg = "Tap label button to create a label annotation for this action.";
                        MoveArrowTo(tutorialObj.transform.GetChild(4).gameObject, msg); // label
                        foreach (Button btn in tutorialObj.GetComponentsInChildren<Button>())
                        {
                            btn.onClick.AddListener(() => {
                                TutorialButtonIsClicked(Steps.WriteLabel);
                                //active all annotations buttons before leaving this step
                                foreach (Button annotBtn in tutorialObj.GetComponentsInChildren<Button>())
                                    annotBtn.interactable = true;
                            });
                            //deactive all buttons except label
                            if (btn != tutorialObj.transform.GetChild(4).GetComponent<Button>())
                                btn.interactable = false;
                        }
                    }
                    break;
                case Steps.WriteLabel:
                    if (actionDetailViewIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("TextEditor");
                        if (!tutorialObj)
                        {
                            TutorialButtonIsClicked(Steps.AddDescription); //jump to add description
                            break;
                        }
                        var acceptBtn = tutorialObj.transform.Find("AcceptButton").gameObject; //accept button
                        var textInputField = tutorialObj.transform.Find("InputField").gameObject;
                        var msg = "Tap here to enter a label text";

                        MoveArrowTo(textInputField, msg);
                        if (textInputField.GetComponent<InputField>().text != "")
                        {
                            TutorialButtonIsClicked(Steps.AcceptAnotation);
                            acceptBtn.SetActive(true);
                        }
                        else
                            acceptBtn.SetActive(false);
                    }
                    break;
                case Steps.AcceptAnotation:
                    if (actionDetailViewIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("TextEditor");
                        if (!tutorialObj)
                        {
                            TutorialButtonIsClicked(Steps.AddDescription); //jump to add description
                            break;
                        }
                        var acceptBtn = tutorialObj.transform.Find("AcceptButton").gameObject;
                        var msg = "Tap this button to save your label.";
                        MoveArrowTo(acceptBtn, msg);
                        acceptBtn.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.ChangeActionName); });
                        acceptBtn.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.ChangeActionName); });

                    }
                    break;
                case Steps.ChangeActionName:
                    if (actionDetailViewIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("ActionTitleInputField");
                        if (!tutorialObj) break;
                        var msg = "Tap here to change the action title.";
                        MoveArrowTo(tutorialObj, msg);

                        var txt = tutorialObj.GetComponent<InputField>().text;
                        if (txt != "" && !txt.Contains("Action Step"))
                        {
                            TutorialButtonIsClicked(Steps.AddDescription);
                        }
                    }
                    break;
                case Steps.AddDescription:
                    if (actionDetailViewIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("DescriptionInputField");
                        if (!tutorialObj) break;
                        var msg = "Tap here to add a description to your action.";
                        MoveArrowTo(tutorialObj, msg);

                        var txt = tutorialObj.GetComponent<InputField>().text;
                        if (txt != "" && !txt.Contains("Add task step description here"))
                            TutorialButtonIsClicked(Steps.DeleteAction);
                    }
                    break;
                case Steps.DeleteAction:
                    if (actionListIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = actionList.transform.GetChild(0).Find("ButtonDelete").gameObject;
                        if (!tutorialObj) break;
                        var msg = "Tap this button to delete this action.";
                        MoveArrowTo(tutorialObj, msg);
                        tutorialObj.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.SaveActivity); });
                        tutorialObj.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.SaveActivity); });
                    }
                    break;
                case Steps.SaveActivity:
                    if (actionListIsOpen && !currentTutorialStep.Done)
                    {
                        var saveBtn = GameObject.Find("Save");
                        var msg = "Tap this button to save your activity on this device.";
                        MoveArrowTo(saveBtn, msg);
                        saveBtn.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.UploadActivity); });
                        saveBtn.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.UploadActivity); });
                    }
                    break;
                case Steps.UploadActivity:
                    if (actionListIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("Upload");
                        if (!tutorialObj) break;
                        var msg = "Tap this button to upload this activity to the cloud.";
                        MoveArrowTo(tutorialObj, msg);
                        tutorialObj.GetComponent<Button>().onClick.AddListener(() => { TutorialButtonIsClicked(Steps.ReturnToActivities); });
                        tutorialObj.GetComponent<PressableButton>().ButtonPressed.AddListener(() => { TutorialButtonIsClicked(Steps.ReturnToActivities); });
                    }
                    break;
                case Steps.ReturnToActivities:
                    tutorialObj = GameObject.Find("Pathway");
                    if (!tutorialObj) break;

                    //here should add an if statment for non headset devices
                    Transform pathwayHomeButton = null;
                    pathwayHomeButton = tutorialObj.transform.FindDeepChild("Caption").transform;

                    if (pathwayHomeButton && !currentTutorialStep.Done)
                    {
                        var msg = "Tap the home button on the floor to return to the activity list.";
                        MoveArrowTo(pathwayHomeButton.gameObject, msg);

                        if (homeButtonPressed)
                            TutorialButtonIsClicked(Steps.LoadActivity);
                    }
                    break;
                case Steps.LoadActivity:
                    if (activitySelectorMenuIsOpen && !currentTutorialStep.Done)
                    {
                        tutorialObj = GameObject.Find("ActivityList");
                        if (!tutorialObj) break;
                        var firstActivityBtn = tutorialObj.transform.GetChild(0).gameObject;
                        var msg = "Tap this activity to download or load it to the scene.";
                        MoveArrowTo(firstActivityBtn, msg);
                        firstActivityBtn.GetComponent<Button>().onClick.AddListener(CloseTutorial);
                        firstActivityBtn.GetComponent<PressableButton>().ButtonPressed.AddListener(CloseTutorial);
                        homeButtonPressed = false;
                    }
                    break;
                default:
                    break;
            }

            //if activity selector is open during the tutorial
            if (activitySelectorMenuIsOpen && currentStep != Steps.LoadActivity)
                foreach (Button btn in GameObject.Find("ActivityList").GetComponentsInChildren<Button>())
                    btn.GetComponent<Button>().onClick.AddListener(() => { CloseTutorial(); });
        }

        private static async Task<GameObject> GetTargetObj(string objName)
        {
            var target = GameObject.Find(objName);
            while (!target)
            {
                target = GameObject.Find(objName);
                await Task.Delay(1);
            }

            return target;
        }
    }

    public class Tutorial
    {
        private bool done;
        private int id;

        public bool Done
        {
            get; set;
        }

        public int ID
        {
            get; set;
        }

        public Tutorial(int str, bool dn = false)
        {
            done = dn;
            id = str;
        }

    }
}