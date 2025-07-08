using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ReadyPlayerMe.Core.Tests
{
    public class SDKLoggerTests
    {
        private const string TAG = nameof(SDKLoggerTests);
        private const string PREFIX = "[Ready Player Me] " + TAG + ": ";
        private const string TEST_LOG_STRING = "TestLog";

        private bool defaultLogEnabledStatus;

        [SetUp]
        public void Setup()
        {
            defaultLogEnabledStatus = SDKLogger.AvatarLoaderLogger.logEnabled;
        }

        [TearDown]
        public void TearDown()
        {
            SDKLogger.AvatarLoaderLogger.logEnabled = defaultLogEnabledStatus;
        }


        [UnityTest]
        public IEnumerator Log_Message_Received()
        {
            var messageReceived = false;

            void OnLogMessageReceived(string logString, string stacktrace, LogType type)
            {
                messageReceived = true;
            }

            Application.logMessageReceived += OnLogMessageReceived;

            SDKLogger.AvatarLoaderLogger.logEnabled = true;
            var wasLoggingEnabled = SDKLogger.IsLoggingEnabled();
            SDKLogger.EnableLogging(true);
            SDKLogger.Log(TAG, TEST_LOG_STRING);

            yield return null;

            Application.logMessageReceived -= OnLogMessageReceived;
            Assert.AreEqual(true, messageReceived, "Message should have been received");
            SDKLogger.EnableLogging(wasLoggingEnabled);
        }

        [UnityTest]
        public IEnumerator Correct_Log_Message_Received()
        {
            var messageReceived = string.Empty;

            void OnLogMessageReceived(string logString, string stacktrace, LogType type)
            {
                messageReceived = logString;
            }

            Application.logMessageReceived += OnLogMessageReceived;
            SDKLogger.AvatarLoaderLogger.logEnabled = true;
            var wasLoggingEnabled = SDKLogger.IsLoggingEnabled();
            SDKLogger.EnableLogging(true);
            SDKLogger.Log(TAG, TEST_LOG_STRING);

            yield return null;
            Application.logMessageReceived -= OnLogMessageReceived;
            Assert.AreEqual(PREFIX + TEST_LOG_STRING, messageReceived);
            SDKLogger.EnableLogging(wasLoggingEnabled);
        }

        [UnityTest]
        public IEnumerator Log_Is_Not_Received_When_Disabled()
        {
            var messageReceived = false;

            void OnLogMessageReceived(string logString, string stacktrace, LogType type)
            {
                messageReceived = true;
            }

            Application.logMessageReceived += OnLogMessageReceived;

            SDKLogger.AvatarLoaderLogger.logEnabled = false;
            var wasLoggingEnabled = SDKLogger.IsLoggingEnabled();
            SDKLogger.EnableLogging(false);
            SDKLogger.Log(TAG, TEST_LOG_STRING);

            yield return null;

            Application.logMessageReceived -= OnLogMessageReceived;
            Assert.AreEqual(false, messageReceived, "Message shouldn't have been received");
            SDKLogger.EnableLogging(wasLoggingEnabled);
        }
    }
}
