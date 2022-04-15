using System.Collections;
using System.Collections.Generic;
using MirageXR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private Color completedColor;

        private ActionListItem actionListItem;
        private ActivityManager activityManager;

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

            activityManager = GenerateGameObjectWithComponent<ActivityManager>("Activity Manager");

            actionListItem.gameObject.SetActive(true);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_GameObjectNameSetToUnused()
        {
            EventManager.ActivateAction("someId");

            Assert.AreEqual("Unused Item", actionListItem.gameObject.name);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_CaptionEmpty()
        {
            EventManager.ActivateAction("someId");

            Assert.AreEqual("", captionLabel.text);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_NumberLabelEmpty()
        {
            EventManager.ActivateAction("someId");

            Assert.AreEqual("", numberLabel.text);
        }

        [Test]
        public void OnActivateAction_ContentNotSet_BackgroundColorStandard()
        {
            EventManager.ActivateAction("someId");

            Assert.AreEqual(standardColor, backgroundImage.color);
        }

        [Test]
        public void OnActivateAction_ContentSet_GameObjectNameSetToId()
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

            EventManager.ActivateAction("someId");

            Assert.AreEqual($"Step-{action.id}", actionListItem.gameObject.name);
        }

        [Test]
        public void OnActivateAction_ContentSet_CaptionLabelSetToTitle()
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

            EventManager.ActivateAction("someId");

            Assert.AreEqual(action.instruction.title, captionLabel.text);
        }

        [Test]
        public void OnActivateAction_ContentSet_NumberLabelSetToDataIndex()
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

                EventManager.ActivateAction("someId");

                Assert.AreEqual((dataIndex + 1).ToString("00"), numberLabel.text);
            }
        }

        [Test]
        public void OnActivateAction_ContentSetToIncompleteAction_StandardBackgroundColorSet()
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

            EventManager.ActivateAction("activeActionId");

            Assert.AreEqual(standardColor, backgroundImage.color);
        }


        [Test]
        public void OnActivateAction_ContentSetToCompletedAction_CompletedBackgroundColorSet()
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

            EventManager.ActivateAction("activeActionId");

            Assert.AreEqual(completedColor, backgroundImage.color);
        }

        private T GenerateGameObjectWithComponent<T>(string name, bool activated = true) where T : MonoBehaviour
        {
            GameObject go = new GameObject(name);
            go.SetActive(activated);
            return go.AddComponent<T>();
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
