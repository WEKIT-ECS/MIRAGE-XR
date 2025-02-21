using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class GhostRecordPlayer : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private Transform _head;

        [SerializeField] private Transform _rightHand;
        [SerializeField] private Transform _leftHand;

        [SerializeField] private Transform _upperSpine;
        [SerializeField] private Transform _lowerSpine;

        public bool IsPlaying => _isPlaying;

        private bool _isPlaying;
        private Transform _anchor;
        private float _cooldown;
        private List<LearningExperienceEngine.GhostDataFrame> _ghostFrames;
        private Coroutine _coroutine;
        private bool _forceStop;

        private void OnDestroy()
        {
            Stop();
        }


        public LearningExperienceEngine.ToggleObject MyToggleObject
        {
            get; set;
        }

        /// <summary>
        /// This function takes a list of SaveData and distributes them accordingly
        /// to the Play method and starts it.
        /// </summary>
        /// <param name="ghostFrames">The recorded data to play.</param>
        /// <param name="anchor">The origin location where the recording will be played.</param>
        /// <param name="loop">If 'true' will repeat record.</param>
        /// <param name="cooldown">Time between capturing ghost position. The default is 'Time.fixedDeltaTime'</param>
        public void Play(List<LearningExperienceEngine.GhostDataFrame> ghostFrames, Transform anchor, bool loop, float? cooldown = null)
        {
            if (ghostFrames == null || ghostFrames.Count <= 0)
            {
                throw new ArgumentException("Can't be null or empty", nameof(ghostFrames));
            }

            if (anchor == null)
            {
                throw new ArgumentException("Can't be null", nameof(anchor));
            }

            Stop();

            _forceStop = false;
            _coroutine = StartCoroutine(PlayIEnumerator(ghostFrames, anchor, loop, cooldown ?? Time.fixedDeltaTime));
        }

        public void Stop()
        {
            _forceStop = true;
            if (_coroutine == null) return;

            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        private IEnumerator PlayIEnumerator(IReadOnlyCollection<LearningExperienceEngine.GhostDataFrame> ghostFrames, Transform anchor, bool loop, float cooldown)
        {
            _isPlaying = true;

            var framerate = Time.fixedDeltaTime;
            var ghostTime = 0f;

            AudioPlayer audioPlayer = null;

            if (MyToggleObject.option.Contains(":"))
            {
                var audioPoi = MyToggleObject.option.Split(':')[1];
                var audioAnnotation = activityManager.ActiveAction.enter.activates.Find(a => a.poi == audioPoi);
                if (audioAnnotation != null)
                {
                    yield return new WaitForSeconds(0.5f);
                    // wait for half a second to give the audio object time to spawn

                    audioPlayer = GameObject.Find(audioAnnotation.poi).GetComponentInChildren<AudioPlayer>();
                    if (audioPlayer)
                    {
                        audioPlayer.PlayAudio();
                        framerate = audioPlayer.getAudioLength() / ghostFrames.Count;
                        // Determine the desired framerate based on the length of the audio and the number of frames, preventing the ghost moving ahead of the audio
                    }
                }
            }

            do
            {
                foreach (var ghostFrame in ghostFrames)
                {
                    if (_forceStop)
                    {
                        break;
                    }

                    if (audioPlayer != null)
                    {
                        if (ghostTime >= audioPlayer.getCurrenttime())
                        {
                            SetFrame(ghostFrame, anchor);
                            yield return new WaitForSeconds(framerate);
                            // if the ghost playback is not behind the audio playback, play the next frame.
                        }
                        ghostTime += framerate;
                        // Update ghost playback time regardless of whether or not the a frame is played, allows frames to be skiped untill the loop catches up with the audio
                    }
                    else
                    {
                        SetFrame(ghostFrame, anchor);
                        yield return new WaitForSeconds(framerate);
                        // If there is no audio then play ghost back at fixedDeltaTime
                    }
                }
            } while (loop && !_forceStop);

            GetComponent<GhosttrackPrefabPlayer>().ShowReplayButton();
            _isPlaying = false;
        }

        private void SetFrame(LearningExperienceEngine.GhostDataFrame ghostFrame, Transform anchor)
        {
            SetPose(_head, ghostFrame.head, anchor);
            SetPose(_upperSpine, ghostFrame.upperSpine, anchor);
            SetPose(_lowerSpine, ghostFrame.lowerSpine, anchor);
            SetPose(_rightHand, ghostFrame.rightHand, anchor);
            SetPose(_leftHand, ghostFrame.leftHand, anchor);
        }

        private static void SetPose(Transform obj, Pose pose, Transform anchor)
        {
            obj.position = anchor.TransformPoint(pose.position);
            obj.rotation = anchor.rotation * pose.rotation;
        }
    }
}