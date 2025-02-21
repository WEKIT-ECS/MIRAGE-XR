using LearningExperienceEngine;
using System.Reflection;
using MirageXR;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
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
        private Color activeColor;
        private Color completedColor;

        private ActionListItem actionListItem;
        private RootObject rootObject;

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            backgroundImage = GenerateGameObjectWithComponent<Image>("Background Image");
            captionLabel = GenerateGameObjectWithComponent<Text>("Caption Label");
            numberLabel = GenerateGameObjectWithComponent<Text>("Number Label");
            deleteButton = GenerateGameObjectWithComponent<Button>("Delete Button");
            checkIcon = GenerateGameObjectWithComponent<Image>("Check Icon");

            standardColor = Color.blue;
            activeColor = Color.red;
            completedColor = Color.green;

            actionListItem = GenerateGameObjectWithComponent<ActionListItem>("Action List Item");
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
        }

        [Test]
        public void UpdateView_ContentNotSet_GameObjectNameSetToUnused()
        {
            actionListItem.UpdateView();

            Assert.AreEqual("Unused Item", actionListItem.gameObject.name);
        }

        [Test]
        public void UpdateView_ContentNotSet_CaptionEmpty()
        {
            actionListItem.UpdateView();

            Assert.AreEqual("", captionLabel.text);
        }

        [Test]
        public void UpdateView_ContentNotSet_NumberLabelEmpty()
        {
            actionListItem.UpdateView();

            Assert.AreEqual("", numberLabel.text);
        }

        [Test]
        public void UpdateView_ContentNotSet_BackgroundColorStandard()
        {
            actionListItem.UpdateView();

            Assert.AreEqual(standardColor, backgroundImage.color);
        }

        [Test]
        public void UpdateView_ContentSet_GameObjectNameSetToId()
        {
            Action action = new Action
            {
                id = "myId",
                instruction = new Instruction
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            actionListItem.UpdateView();

            Assert.AreEqual($"Step-{action.id}", actionListItem.gameObject.name);
        }

        [Test]
        public void UpdateView_ContentSet_CaptionLabelSetToTitle()
        {
            Action action = new Action
            {
                id = "myId",
                instruction = new Instruction
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            actionListItem.UpdateView();

            Assert.AreEqual(action.instruction.title, captionLabel.text);
        }

        [Test]
        public void UpdateView_ContentSet_NumberLabelSetToDataIndex()
        {
            int[] testCases = { 0, 5, 25 };

            Action action = new Action
            {
                id = "myId",
                instruction = new Instruction
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            foreach (int dataIndex in testCases)
            {
                actionListItem.DataIndex = dataIndex;

                actionListItem.UpdateView();

                Assert.AreEqual((dataIndex + 1).ToString("00"), numberLabel.text);
            }
        }

        [Test]
        public void UpdateView_ContentSetToIncompleteAction_StandardBackgroundColorSet()
        {
            Action action = new Action
            {
                id = "someId",
                instruction = new Instruction
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            Action activeAction = new Action
            {
                id = "activeActionId"
            };

            SetPrivateProperty(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld, "ActiveAction", activeAction);

            actionListItem.UpdateView();

            Assert.AreEqual(standardColor, backgroundImage.color);
        }


        [Test]
        public void UpdateView_ContentSetToCompletedAction_CompletedBackgroundColorSet()
        {
            Action action = new Action
            {
                id = "someId",
                instruction = new Instruction
                {
                    title = "ActionTitle"
                },
                isCompleted = true
            };

            actionListItem.Content = action;

            Action activeAction = new Action
            {
                id = "activeActionId"
            };

            SetPrivateProperty(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld, "ActiveAction", activeAction);

            actionListItem.UpdateView();

            Assert.AreEqual(completedColor, backgroundImage.color);
        }

        private static T GenerateGameObjectWithComponent<T>(string name) where T : MonoBehaviour
        {
            return new GameObject(name).AddComponent<T>();
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
