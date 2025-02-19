using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    [RequireComponent(typeof(GhostRecordPlayer))]
    public class GhosttrackPrefabPlayer : MirageXRPrefab
    {

        [SerializeField] private Button _replayButton;

        private GhostRecordPlayer _ghost;
        private List<LearningExperienceEngine.GhostDataFrame> _recordedGhostData;
        private Transform _anchor;
        private int _totalFrames;
        private bool _loop;

        private void Start()
        {
            _replayButton.onClick.AddListener(PlayRecording);
        }

        public void ShowReplayButton()
        {
            _replayButton.gameObject.SetActive(true);
        }

        public override bool Init(LearningExperienceEngine.ToggleObject content)
        {
            _ghost = gameObject.GetComponent<GhostRecordPlayer>();
            _ghost.MyToggleObject = content;
            _anchor = GameObject.Find(content.id).transform;  // TODO: possible NRE. replace with direct ref

            if (string.IsNullOrEmpty(content.url))
            {
                Debug.Log("Content URL not provided.");
                return false;
            }

            if (!SetParent(content)) // Try to set the parent and if it fails, terminate initialization.
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            name = content.predicate;  // Set name. Please don't modify so that the ghosttrack can be deactivated...
            _loop = content.option.Contains("loop");

            if (GhostRecorder.TryLoadFromFile(GetFullFilePathFromUrl(content.url), out _recordedGhostData))
            {
                PlayRecording();
                return true;
            }

            Debug.Log("Error loading ghost recording");
            return false;
        }

        public void PlayRecording()
        {
            Debug.Log($"PlayRecording: there are {_recordedGhostData.Count} ghost frames");

            _ghost.gameObject.SetActive(true);
            _ghost.Play(_recordedGhostData, _anchor, _loop);
            _replayButton.gameObject.SetActive(false);

            TutorialManager.Instance.InvokeEvent(TutorialManager.TutorialEvent.GHOST_REPLAYED);
        }

        private static string GetFullFilePathFromUrl(string url)
        {
            var fileName = url.Replace("resources://", string.Empty);
            fileName = fileName.Replace("http://", string.Empty);

            return Path.Combine(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath, fileName);
        }
    }
}
