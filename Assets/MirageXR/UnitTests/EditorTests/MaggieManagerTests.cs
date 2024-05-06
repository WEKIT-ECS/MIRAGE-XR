using FakeItEasy;
using MirageXR;
using NUnit.Framework;
using System;
using UnityEditor.SceneManagement;

namespace Tests
{
    public class MaggieManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        }


        [Test]
        public void Constructor_NoTts_TtsInitialized()
        {
            MaggieManager maggie = new MaggieManager();
            Assert.IsNotNull(maggie.TTSService);
        }

        [Test]
        public void Constructor_TtsGiven_TtsUsed()
        {
            ITextToSpeechService expected = A.Fake<ITextToSpeechService>();
            MaggieManager maggie = new MaggieManager(expected);
            Assert.AreEqual(expected, maggie.TTSService);
        }

        [Test]
        public void Speak_NonEmptyText_Speaks()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            A.CallTo(() => tts.IsSpeaking()).Returns(false);
            string msg = "This is a test";
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Speak(msg);
            A.CallTo(() => tts.StartSpeaking(msg)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Speak_TextGiven_StopsSpeakingFirst()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            string msg = "This is a test";
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Speak(msg);
            A.CallTo(() => tts.StopSpeaking()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Speak_StillSpeaking_DoNotSpeak()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            A.CallTo(() => tts.IsSpeaking()).Returns(true);
            string msg = "This is a test";
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Speak(msg);
            A.CallTo(() => tts.StartSpeaking(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void Speak_EmptyText_DoNotSpeak()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            A.CallTo(() => tts.IsSpeaking()).Returns(false);
            string msg = "";
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Speak(msg);
            A.CallTo(() => tts.StartSpeaking(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void ActivityReady_SpeaksActivityReadyMessage()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            MaggieManager maggie = new MaggieManager(tts);
            maggie.ActivityReady();
            A.CallTo(() => tts.StartSpeaking(
                A<string>.That.Matches(text => ArrayContains(maggie.activityReadyMessages, text))))
                .MustHaveHappened();
        }

        [Test]
        public void Stop_StopsTTS()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Stop();
            A.CallTo(() => tts.StopSpeaking()).MustHaveHappened();
        }

        [Test]
        public void Error_SpeaksErrorMessage()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Error();
            A.CallTo(() => tts.StartSpeaking(
                A<string>.That.Matches(text => ArrayContains(maggie.errorMessages, text))))
                .MustHaveHappened();
        }

        [Test]
        public void Ok_SpeaksOkMessage()
        {
            ITextToSpeechService tts = A.Fake<ITextToSpeechService>();
            MaggieManager maggie = new MaggieManager(tts);
            maggie.Ok();
            A.CallTo(() => tts.StartSpeaking(
                A<string>.That.Matches(text => ArrayContains(maggie.okMessages, text))))
                .MustHaveHappened();
        }

        private bool ArrayContains(string[] array, string text)
        {
            return Array.Exists(array, element => element.Equals(text));
        }
    }
}
