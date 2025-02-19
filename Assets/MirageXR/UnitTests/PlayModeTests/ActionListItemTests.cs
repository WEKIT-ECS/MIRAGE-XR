using System.Reflection;
using MirageXR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tests
{
    public class ActionListItemTests
    {
        private Image backgroundImage;
        private Text captionLabel;
        private Text numberLabel;
        private Button deleteButton;
        private Image checkIcon;

        private Color standardColor;
        private Color completedColor;

        private ActionListItem actionListItem;
        private RootObject rootObject;

        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene("TestScene");

            backgroundImage = GenerateGameObjectWithComponent<Image>("Background Image");
            captionLabel = GenerateGameObjectWithComponent<Text>("Caption Label");
            numberLabel = GenerateGameObjectWithComponent<Text>("Number Label");
            deleteButton = GenerateGameObjectWithComponent<Button>("Delete Button");
            checkIcon = GenerateGameObjectWithComponent<Image>("Check Icon");

            standardColor = Color.blue;
            completedColor = Color.green;

            // create script on deactivated gameobject so that initialization only runs after fields have been set up
            actionListItem = GenerateGameObjectWithComponent<ActionListItem>("Action List Item", false);
            SetPrivateField(actionListItem, "backgroundImage", backgroundImage);
            SetPrivateField(actionListItem, "captionLabel", captionLabel);
            SetPrivateField(actionListItem, "numberLabel", numberLabel);
            SetPrivateField(actionListItem, "standardColor", standardColor);
            SetPrivateField(actionListItem, "completedColor", completedColor);
            SetPrivateField(actionListItem, "deleteButton", deleteButton);
            SetPrivateField(actionListItem, "checkIcon", checkIcon);

            if (!RootObject.Instance)
            {
                rootObject = GenerateGameObjectWithComponent<RootObject>("root");
                CallPrivateMethod(rootObject, "Awake");
                CallPrivateMethod(rootObject, "Initialization");
            }
            else
            {
                rootObject = RootObject.Instance;
            }
            actionListItem.gameObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadSceneAsync("TestScene");
        }
        
        [Test]
        public void OnActivateAction_ContentNotSet_GameObjectNameSetToUnused()
        {
            LearningExperienceEngine.EventManager.ActivateAction("someId");

            Assert.AreEqual("Unused Item", actionListItem.gameObject.name);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_CaptionEmpty()
        {
            LearningExperienceEngine.EventManager.ActivateAction("someId");

            Assert.AreEqual("", captionLabel.text);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_NumberLabelEmpty()
        {
            LearningExperienceEngine.EventManager.ActivateAction("someId");

            Assert.AreEqual("", numberLabel.text);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_BackgroundColorStandard()
        {
            LearningExperienceEngine.EventManager.ActivateAction("someId");

            Assert.AreEqual(standardColor, backgroundImage.color);
        }

        [Test]
        public void OnActivateAction_ContentSet_GameObjectNameSetToId()
        {
            LearningExperienceEngine.Action action = new LearningExperienceEngine.Action()
            {
                id = "myId",
                instruction = new LearningExperienceEngine.Instruction()
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            LearningExperienceEngine.EventManager.ActivateAction("someId");

            Assert.AreEqual($"Step-{action.id}", actionListItem.gameObject.name);
        }

        [Test]
        public void OnActivateAction_ContentSet_CaptionLabelSetToTitle()
        {
            LearningExperienceEngine.Action action = new LearningExperienceEngine.Action()
            {
                id = "myId",
                instruction = new LearningExperienceEngine.Instruction()
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            LearningExperienceEngine.EventManager.ActivateAction("someId");

            Assert.AreEqual(action.instruction.title, captionLabel.text);
        }

        [Test]
        public void OnActivateAction_ContentSet_NumberLabelSetToDataIndex()
        {
            int[] testCases = new int[] { 0, 5, 25 };

            LearningExperienceEngine.Action action = new LearningExperienceEngine.Action()
            {
                id = "myId",
                instruction = new LearningExperienceEngine.Instruction()
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            foreach (int dataIndex in testCases)
            {
                actionListItem.DataIndex = dataIndex;

                LearningExperienceEngine.EventManager.ActivateAction("someId");

                Assert.AreEqual((dataIndex + 1).ToString("00"), numberLabel.text);
            }
        }

        [Test]
        public void OnActivateAction_ContentSetToIncompleteAction_StandardBackgroundColorSet()
        {
            LearningExperienceEngine.Action action = new LearningExperienceEngine.Action()
            {
                id = "someId",
                instruction = new LearningExperienceEngine.Instruction()
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            LearningExperienceEngine.Action activeAction = new LearningExperienceEngine.Action()
            {
                id = "activeActionId"
            };

           // rootObject.activityManager ??= new ActivityManager();
            SetPrivateProperty(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld, "ActiveAction", activeAction);

            LearningExperienceEngine.EventManager.ActivateAction("activeActionId");

            Assert.AreEqual(standardColor, backgroundImage.color);
        }


        [Test]
        public void OnActivateAction_ContentSetToCompletedAction_CompletedBackgroundColorSet()
        {
            LearningExperienceEngine.Action action = new LearningExperienceEngine.Action()
            {
                id = "someId",
                instruction = new LearningExperienceEngine.Instruction()
                {
                    title = "ActionTitle"
                },
                isCompleted = true
            };

            actionListItem.Content = action;

            LearningExperienceEngine.Action activeAction = new LearningExperienceEngine.Action()
            {
                id = "activeActionId"
            };

            //rootObject.activityManager ??= new ActivityManager();
            SetPrivateProperty(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld, "ActiveAction", activeAction);

            LearningExperienceEngine.EventManager.ActivateAction("activeActionId");

            Assert.AreEqual(completedColor, backgroundImage.color);
        }

        private static T GenerateGameObjectWithComponent<T>(string name, bool activated = true) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            go.SetActive(activated);
            return go.AddComponent<T>();
        }

        private static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(obj, value);
        }

        private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
        {
            obj.GetType().GetProperty(propertyName)?.SetValue(obj, value);
        }

        private static void CallPrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(obj, parameters);
        }
    }
}
