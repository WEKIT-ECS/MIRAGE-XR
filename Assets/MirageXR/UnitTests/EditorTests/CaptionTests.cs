using System;
using LearningExperienceEngine;
using MirageXR;
using NUnit.Framework;
using UnityEngine;
using EventManager = MirageXR.EventManager;

namespace Tests
{
    public class CaptionTests
    {
        private bool _originalCaptionSetting;

        [SetUp]
        public void SetUp()
        {
            _originalCaptionSetting = UserSettings.showCaptions;
        }

        [TearDown]
        public void TearDown()
        {
            UserSettings.showCaptions = _originalCaptionSetting;

            if (CaptionController.Instance != null)
            {
                UnityEngine.Object.DestroyImmediate(CaptionController.Instance.gameObject);
            }
        }

        [Test]
        public void UserSettings_ShowCaptions_CanSetAndGet()
        {
            UserSettings.showCaptions = true;
            Assert.IsTrue(UserSettings.showCaptions);

            UserSettings.showCaptions = false;
            Assert.IsFalse(UserSettings.showCaptions);
        }

        [Test]
        public void UserSettings_CaptionsSettingChanged_EventFiresOnValueChange()
        {
            bool eventFired = false;
            bool eventValue = false;

            Action<bool> handler = val =>
            {
                eventFired = true;
                eventValue = val;
            };

            UserSettings.CaptionsSettingChanged += handler;
            try
            {
                UserSettings.showCaptions = true;
                Assert.IsTrue(eventFired);
                Assert.IsTrue(eventValue);

                eventFired = false;
                UserSettings.showCaptions = false;
                Assert.IsTrue(eventFired);
                Assert.IsFalse(eventValue);
            }
            finally
            {
                UserSettings.CaptionsSettingChanged -= handler;
            }
        }

        [Test]
        public void EventManager_CaptionEvents_FireCorrectly()
        {
            bool showFired = false;
            string receivedText = null;
            float receivedDuration = 0f;
            string receivedSpeaker = null;

            EventManager.ShowCaptionDelegate showHandler = (text, duration, speaker) =>
            {
                showFired = true;
                receivedText = text;
                receivedDuration = duration;
                receivedSpeaker = speaker;
            };

            bool hideFired = false;
            EventManager.HideCaptionDelegate hideHandler = () =>
            {
                hideFired = true;
            };

            EventManager.OnShowCaption += showHandler;
            EventManager.OnHideCaption += hideHandler;

            try
            {
                EventManager.ShowCaption("Test message", 4.5f, "Instructor");
                Assert.IsTrue(showFired);
                Assert.AreEqual("Test message", receivedText);
                Assert.AreEqual(4.5f, receivedDuration);
                Assert.AreEqual("Instructor", receivedSpeaker);

                EventManager.HideCaption();
                Assert.IsTrue(hideFired);
            }
            finally
            {
                EventManager.OnShowCaption -= showHandler;
                EventManager.OnHideCaption -= hideHandler;
            }
        }

        [Test]
        public void CaptionController_Instantiates_AndHandlesShowCaption()
        {
            UserSettings.showCaptions = true;

            var controller = CaptionController.EnsureExists();
            Assert.IsNotNull(controller);
            Assert.AreEqual(controller, CaptionController.Instance);

            // Trigger ShowCaption
            controller.ShowCaption("Welcome to the procedure.", 3f, "Virtual Instructor");

            // Verify clean hide
            controller.HideCaption();
            controller.Clear();
        }

        [Test]
        public void CaptionController_WhenDisabledInSettings_DoesNotShow()
        {
            UserSettings.showCaptions = false;

            var controller = CaptionController.EnsureExists();
            Assert.IsNotNull(controller);

            // Show caption when disabled should be a no-op
            controller.ShowCaption("This should not show", 3f, "Instructor");
        }
    }
}
