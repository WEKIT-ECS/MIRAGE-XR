using MirageXR;
using NUnit.Framework;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
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
        private ActivityManager activityManager;

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

            activityManager = GenerateGameObjectWithComponent<ActivityManager>("ActivityManager");
            activityManager.GetType().GetProperty("Instance").SetValue(null, activityManager);
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
            Action action = new Action()
            {
                id = "myId",
                instruction = new Instruction()
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
            Action action = new Action()
            {
                id = "myId",
                instruction = new Instruction()
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
            int[] testCases = new int[] { 0, 5, 25 };

            Action action = new Action()
            {
                id = "myId",
                instruction = new Instruction()
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
            Action action = new Action()
            {
                id = "someId",
                instruction = new Instruction()
                {
                    title = "ActionTitle"
                }
            };

            actionListItem.Content = action;

            Action activeAction = new Action()
            {
                id = "activeActionId"
            };

            SetPrivateProperty(activityManager, "ActiveAction", activeAction);

            actionListItem.UpdateView();

            Assert.AreEqual(standardColor, backgroundImage.color);
        }


        [Test]
        public void UpdateView_ContentSetToCompletedAction_CompletedBackgroundColorSet()
        {
            Action action = new Action()
            {
                id = "someId",
                instruction = new Instruction()
                {
                    title = "ActionTitle"
                },
                isCompleted = true
            };

            actionListItem.Content = action;

            Action activeAction = new Action()
            {
                id = "activeActionId"
            };

            SetPrivateProperty(activityManager, "ActiveAction", activeAction);

            actionListItem.UpdateView();

            Assert.AreEqual(completedColor, backgroundImage.color);
        }

        private T GenerateGameObjectWithComponent<T>(string name) where T : MonoBehaviour
        {
            return new GameObject(name).AddComponent<T>();
        }

        private void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic
    | System.Reflection.BindingFlags.Instance).SetValue(obj, value);
        }

        private void SetPrivateProperty<T>(object obj, string propertyName, T value)
        {
            obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        }
    }
}
