using LearningExperienceEngine;
using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TiltBrush;
using UnityEngine;

namespace MirageXR
{

    public class DrawingPlayer : MirageXRPrefab
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private LearningExperienceEngine.ToggleObject _obj;

        [SerializeField] private Tiltbrush tiltbrushPrefab;
        private Tiltbrush tiltInstance;
        private SimpleSnapshot TiltSnapshot;

        private void OnDestroy()
        {
            tiltInstance.UnsubscribeComponent(this);
        }

        /// <summary>
        /// This method starts playback of the drawing file.
        /// If the drawing is already being played, it will be restarted from the beginning.
        /// </summary>
        public void PlayDrawing()
        {
            if (tiltInstance == null || TiltSnapshot == null)
            {
                return;
            }

            if (TiltSnapshot.IsPlaying && !TiltSnapshot.IsDonePlaying)
            {
                TiltSnapshot.Stop();
            }

            TiltSnapshot.Play();
        }

        /// <summary>
        /// Pause the playback of drawing, or if already paused, resume play
        /// </summary>
        public void PauseDrawing()
        {
            if (tiltInstance == null || TiltSnapshot == null)
            {
                return;
            }

            if (TiltSnapshot.IsPlaying && !TiltSnapshot.IsDonePlaying)
                TiltSnapshot.Pause();
            else if (!TiltSnapshot.IsDonePlaying && !TiltSnapshot.IsPlaying)
                TiltSnapshot.Play();
        }


        /// <summary>
        /// Stop the playback of drawing
        /// </summary>
        public void StopDrawing()
        {
            if (tiltInstance == null || TiltSnapshot == null)
            {
                return;
            }

            if (!TiltSnapshot.IsDonePlaying)
            {
                TiltSnapshot.Stop();
            }
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            _obj = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            if (Tiltbrush.Instance != null)
                tiltInstance = Tiltbrush.Instance;
            else
                tiltInstance = Instantiate(tiltbrushPrefab);

            tiltInstance.SubscribeComponent(this);
            tiltInstance.transform.SetParent(transform);
            tiltInstance.transform.localPosition = Vector3.zero;
            tiltInstance.transform.localRotation = Quaternion.identity;
            tiltInstance.ClearScene();

            // Set name.
            name = obj.predicate;

            CreateDrawingPlayer();

            // If all went well, return true.
            return true;
        }

        /// <summary>
        /// This method creates an audio player
        /// Destroys any already existing audio player in this GameObject.
        /// PlayAudio() must be called to start the audio playback.
        /// </summary>
        public void CreateDrawingPlayer()
        {
            var succes = LoadDrawing();
            if (!succes)
                return;

            PlayDrawing();
        }

        private bool LoadDrawing()
        {
            try
            {
                const string httpPrefix = "http://";
                var completeFilename = !_obj.url.StartsWith(httpPrefix)
                    ? Path.Combine(Application.persistentDataPath, _obj.url)
                    : Path.Combine(activityManager.ActivityPath,
                        Path.GetFileName(_obj.url.Remove(0, httpPrefix.Length)));
                
                TiltSnapshot = tiltInstance.ImportSnapshotFromFile(completeFilename);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("An error occured during import of drawing file. Aborting. Error: " + e.Message);
                return false;
            }
        }
    }
}
