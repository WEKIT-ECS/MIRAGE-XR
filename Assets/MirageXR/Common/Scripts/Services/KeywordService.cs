using i5.Toolkit.Core.ServiceCore;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_WSA
using UnityEngine.Windows.Speech;
using Microsoft.MixedReality.Toolkit.Input;
#endif
using Vuforia;

namespace MirageXR
{
    /// <summary>
    /// One keyword servcie to rule them all!
    /// SpeechService.cs from recorder modified to work with both recorder and player.
    /// </summary>
    public class KeywordService : IService
    {
#if UNITY_WSA
        KeywordRecognizer keywordRecognizer = null;
#endif
        Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

        private string[] _prompts = { "Hey Presto", "Hi Mirage", "Sim Sala Bim", "Alakazam" };

        /// <summary>
        /// Called by the ServiceManager
        /// initializes the list of keywords to listen to and starts listening.
        /// </summary>
        /// <param name="owner"></param>
        public void Initialize(IServiceManager owner)
        {
#if UNITY_WSA
            AddKeywords();
#endif
        }


        private void AddPromptsToKeyword(string keyword, System.Action callback)
        {
            foreach (var prompt in _prompts)
            {
                keywords.Add($"{prompt} {keyword}", () =>
                {
                    callback();
                });
            }
        }


        /// <summary>
        /// Called by the ServiceManager to clean up the service
        /// </summary>
        public void Cleanup()
        {
#if UNITY_WSA

            if (keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }
            keywordRecognizer.Dispose();
#endif
        }


        void AddKeywords()
        {
#if UNITY_WSA
            // Added for making all the player crap to work. Using events so no need to make player/recorder checks here...
            AddPromptsToKeyword("Show Action List", () => EventManager.ActionlistToggleByVoice(true));// TODO this command not working
            AddPromptsToKeyword("Hide Action List", () => EventManager.ActionlistToggleByVoice(false));// TODO this command not working
            AddPromptsToKeyword("Move Action List", () => EventManager.MoveActionList());
            AddPromptsToKeyword("Lock Action List", () => EventManager.LockMenuByVoice());
            AddPromptsToKeyword("Unlock Action List", () => EventManager.ReleaseMenuByVoice());
            AddPromptsToKeyword("Add Action", () => EventManager.AddActionByVoice());
            AddPromptsToKeyword("Delete Action", () => EventManager.DeleteActionByVoice());
            AddPromptsToKeyword("Next", () => EventManager.NextByVoice());
            AddPromptsToKeyword("Back", () => EventManager.BackByVoice());
            AddPromptsToKeyword("New Activity", () => EventManager.StartByVoice());// TODO this command not working
            AddPromptsToKeyword("Show Activity List", () => EventManager.ShowActivitySelectionMenu());
            AddPromptsToKeyword("Hide Activity List", () => EventManager.HideActivitySelectionMenu());
            AddPromptsToKeyword("Move Activity List", () => EventManager.MoveActiovityList());
            AddPromptsToKeyword("Open Annotation List", () => EventManager.OpenAnnotationByVoice());
            AddPromptsToKeyword("Login", () => EventManager.LoginByVoice());
            AddPromptsToKeyword("Register", () => EventManager.RegisterByVoice());
            AddPromptsToKeyword("Save", () => EventManager.SaveActivityByVoice());
            AddPromptsToKeyword("Upload", () => EventManager.UploadActivityByVoice());
            AddPromptsToKeyword("Turn on Edit Mode", () => { EventManager.NotifyEditModeChanged(true); ActivityEditor.Instance.SetEditorState(true); });
            AddPromptsToKeyword("Turn off Edit Mode", () => { EventManager.NotifyEditModeChanged(false); ActivityEditor.Instance.SetEditorState(false); });
            AddPromptsToKeyword("Add Image", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.IMAGE); });
            AddPromptsToKeyword("Add Act", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.ACT); });
            AddPromptsToKeyword("Add Glyph", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.ACT); });
            AddPromptsToKeyword("Add Audio", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.AUDIO); });
            AddPromptsToKeyword("Add Video", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.VIDEO); });
            AddPromptsToKeyword("Add Visual effect", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.VFX); });
            AddPromptsToKeyword("Add Character", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.CHARACTER); });
            AddPromptsToKeyword("Add Ghost", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.GHOST); });
            AddPromptsToKeyword("Add Label", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.LABEL); });
            AddPromptsToKeyword("Add Pick and Place", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.PICKANDPLACE); });
            AddPromptsToKeyword("Add Image Marker", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.IMAGEMARKER); });
            AddPromptsToKeyword("Add Model", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.MODEL); });
            AddPromptsToKeyword("Add Plugin", () => { ActionEditor.Instance.OnAnnotationAddItemSelected(ContentType.PLUGIN); });

            // Tell the KeywordRecognizer about our keywords.
            Debug.Log("Registering keywords");
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
            Debug.Log("Keywords registered");

            // Register a callback for the KeywordRecognizer and start recognizing!
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            keywordRecognizer.Start();
#endif
        }

#if UNITY_WSA
        /// <summary>
        /// handler method for recognized keywords.
        /// </summary>
        /// <param name="args"></param>
        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            System.Action keywordAction;
            if (keywords.TryGetValue(args.text, out keywordAction))
            {
                keywordAction.Invoke();
            }
        }
#endif

        /// <summary>
        /// called, when microphone is needed for audio recording or other purposes.
        /// </summary>
        public void PauseRecognizer()
        {
#if UNITY_WSA
            if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }
#endif
        }

        /// <summary>
        /// called, when microphone can be used for speech recognition again.
        /// </summary>
        public void ContinueRecognizer()
        {
#if UNITY_WSA
            if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Start();
            }
#endif
        }

    }
}