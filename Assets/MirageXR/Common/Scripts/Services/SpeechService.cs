using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
#if UNITY_WSA
using UnityEngine.Windows.Speech;
#endif
using HoloToolkit.Unity;
using Microsoft.MixedReality.Toolkit.Input;
using Vuforia;
using i5.Toolkit.Core.ServiceCore;

// TO DO: this should be integrated with KeywordManager.cs into one common manager
// RK: 2018-06-12: re-activated speechmanager, as player and recorder require different keywords to be active

namespace MirageXR
{
    /// <summary>
    /// class to handle speech commands as alternative input.
    /// 
    /// </summary>
    public class SpeechService : IService
    {
        private GazeProvider gazeProvider;
#if UNITY_WSA
        KeywordRecognizer keywordRecognizer = null;
#endif
        Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

        /// <summary>
        /// initializes the list of keywords to listen to and starts listening.
        /// </summary>
        public void Initialize(IServiceManager owner)
        {
            AddKeywords();
        }

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
            ActivityRecorderService activityService = ServiceManager.GetService<ActivityRecorderService>();

            keywords.Add("test", () =>
            {
                activityService.UpdateDataModel();
            });

            keywords.Add("App Menu", () =>
            {
                var focusObject = gazeProvider.GazeTarget;
                if (focusObject != null)
                {
                    // Call the OpenMenu method on just the focused object.
                    focusObject.SendMessage("OpenMenu");
                }
            });

            keywords.Add("Remove Annotation", () =>
            {
                var focusObject = GazeGestureManager.Instance.FocusedObject;
                if (focusObject != null)
                {
                    // Call the OnDrop method on just the focused object.
                    focusObject.SendMessage("RemoveAnnotation");
                }
            });

            keywords.Add("Remove Task Station", () =>
            {
                var focusObject = GazeGestureManager.Instance.FocusedObject;
                if (focusObject != null)
                {
                    // Call the OnDrop method on just the focused object.
                    focusObject.SendMessage("RemoveTaskStation");
                }
            });

            

            keywords.Add("Upload Arlem", () =>
            {
                Debug.Log("Upload ARLEM started.");
                Maggie.Speak("Exporting and uploading ARLEM");
                Debug.Log("Saving ARLEM ...");
                activityService.UpdateDataModel();
                System.DateTime nowDate = System.DateTime.Now;
                string dirBase = "session-" + nowDate.ToString("yyyy-MM-dd_HH-mm-ss");
                string folder = Application.persistentDataPath + "/" + dirBase;
                //string folder = activityService.currentArlemFolder;
                int idx = folder.LastIndexOf('/') + 1;
                string arlemId = folder.Substring(idx);
                Debug.Log("Zipping and Uploading " + folder + " to " + arlemId);
                Network.Upload(folder, arlemId, httpStatusCode => {
                    Maggie.Speak(httpStatusCode == HttpStatusCode.OK
                        ? "Save complete."
                        : $"Uploading ARLEM failed. Check your system administrator. {httpStatusCode}");
                });
                Debug.Log("Upload ARLEM done.");
            });

#if UNITY_WSA
            // Tell the KeywordRecognizer about our keywords.
            keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

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