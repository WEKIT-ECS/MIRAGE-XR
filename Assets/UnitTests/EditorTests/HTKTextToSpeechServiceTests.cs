using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FakeItEasy;
using HoloToolkit.Unity;
using i5.Toolkit.Core.ServiceCore;
using MirageXR;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HTKTextToSpeechServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        [Test]
        public void Constructor_VoiceGiven_VoiceSet()
        {
            TextToSpeechVoice expected = TextToSpeechVoice.David;
            HTKTextToSpeechService htk = new HTKTextToSpeechService(expected);
            Assert.AreEqual(expected, htk.Voice);
        }

        [Test]
        public void Initialize_TextToSpeechObjectCreated()
        {
            HTKTextToSpeechService htk = new HTKTextToSpeechService();
            htk.Initialize(A.Fake<IServiceManager>());
            GameObject go = GameObject.Find("TextToSpeech Manager");
            Assert.True(go != null);
            TextToSpeech tts = go.GetComponent<TextToSpeech>();
            Assert.True(tts != null);
        }

        [Test]
        public void Initialize_VoiceAssignedInConstructor_VoiceApplied()
        {
            TextToSpeechVoice expected = TextToSpeechVoice.David;
            HTKTextToSpeechService htk = new HTKTextToSpeechService(expected);
            htk.Initialize(A.Fake<IServiceManager>());
            GameObject go = GameObject.Find("TextToSpeech Manager");
            Assert.True(go != null);
            TextToSpeech tts = go.GetComponent<TextToSpeech>();
            Assert.True(tts != null);
            Assert.AreEqual(expected, tts.Voice);
        }

        [Test]
        public void Voice_VoiceAssignedBeforeInitialize_VoiceApplied()
        {
            TextToSpeechVoice expected = TextToSpeechVoice.David;
            HTKTextToSpeechService htk = new HTKTextToSpeechService();
            htk.Voice = expected;
            htk.Initialize(A.Fake<IServiceManager>());
            GameObject go = GameObject.Find("TextToSpeech Manager");
            TextToSpeech tts = go.GetComponent<TextToSpeech>();
            Assert.AreEqual(expected, tts.Voice);
        }

        [Test]
        public void Voice_VoiceAssignedAfterInitialize_VoiceApplied()
        {
            TextToSpeechVoice expected = TextToSpeechVoice.David;
            HTKTextToSpeechService htk = new HTKTextToSpeechService();
            htk.Initialize(A.Fake<IServiceManager>());
            htk.Voice = expected;
            GameObject go = GameObject.Find("TextToSpeech Manager");
            TextToSpeech tts = go.GetComponent<TextToSpeech>();
            Assert.AreEqual(expected, tts.Voice);
        }
    }
}
