using LearningExperienceEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TiltBrush;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MirageXR
{
    public class DrawingEditor : MonoBehaviour
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        [SerializeField] private MrtkSimpleBtn startRecordingButton;
        [SerializeField] private MrtkSimpleBtn stopRecordingButton;
        [SerializeField] private MrtkSimpleBtn acceptButton;
        [SerializeField] private MrtkSimpleBtn pauseButton;
        [SerializeField] private MrtkSimpleBtn playButton;
        [SerializeField] private MrtkSimpleBtn closeButton;

        [SerializeField] private Text timerText;

        [SerializeField] private Tiltbrush tiltPrefab;

        private Transform annotationStartingPoint;

        private Tiltbrush tiltInstance;
        private SimpleSnapshot tiltSnapshot;

        private bool isPlaying = false;
        private bool isRecording = false;
        private bool isPaused = false;
        private LearningExperienceEngine.Action action;
        private LearningExperienceEngine.ToggleObject annotationToEdit;

        void Awake()
        {
            startRecordingButton.onClick.RemoveAllListeners();
            startRecordingButton.onClick.AddListener(StartRecording);

            stopRecordingButton.onClick.RemoveAllListeners();
            stopRecordingButton.onClick.AddListener(StopRecording);

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(Play);

            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(Pause);

            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(Create);

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        void Update()
        {
            if (isPlaying && tiltSnapshot.IsDonePlaying)
            {
                isPlaying = false;
                isPaused = false;
                UpdateUI();
            }
        }

        string GetExistingDrawing()
        {
            var drawingName = annotationToEdit.url;
            const string httpPrefix = "http://";
            string originalFileName = !drawingName.StartsWith(httpPrefix)
                ? Path.Combine(Application.persistentDataPath, drawingName)
                : Path.Combine(activityManager.ActivityPath,
                    Path.GetFileName(drawingName.Remove(0, httpPrefix.Length)));

            return Path.Combine(activityManager.ActivityPath, drawingName);
        }

        void Play()
        {
            tiltSnapshot.Play();
            isPlaying = true;
            isPaused = false;
            UpdateUI();
        }

        void Pause()
        {
            tiltSnapshot.Pause();
            isPaused = true;
            UpdateUI();
        }

        void UpdateUI()
        {
            startRecordingButton.interactable = !isPlaying || (isPlaying && isPaused);

            startRecordingButton.gameObject.SetActive(!isRecording);
            stopRecordingButton.gameObject.SetActive(!startRecordingButton.gameObject.activeSelf);

            playButton.interactable = tiltSnapshot != null && !isRecording;
            playButton.gameObject.SetActive(!isPlaying || isPaused);
            pauseButton.gameObject.SetActive(!playButton.gameObject.activeSelf);

            acceptButton.gameObject.SetActive(tiltSnapshot != null && !isRecording);
        }

        public void SetAnnotationStartingPoint(Transform startingPoint)
        {
            annotationStartingPoint = startingPoint;
        }

        public async void Create()
        {
            StopRecording();

            var filename = $"MirageXR_Drawing_{System.DateTime.Now.ToFileTimeUtc()}.tilt";

            string fullFilePath = Path.Combine(activityManager.ActivityPath, filename);
            await tiltSnapshot.WriteToFile(fullFilePath);

            // Delete old file
            if (annotationToEdit != null)
            {
                var originalFilePath = GetExistingDrawing();
                if (File.Exists(originalFilePath))
                {
                    LearningExperienceEngine.EventManager.DeactivateObject(annotationToEdit);
                    File.Delete(originalFilePath);
                }
            }
            else
            {
                var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
                LearningExperienceEngine.Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
                GameObject originT = GameObject.Find(detectable.id);

                // Offset should always be 0. Positional data are store in the drawing strokes.
                var offset = Vector3.zero;

                annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(action, offset);
                annotationToEdit.predicate = "drawing";
            }

            annotationToEdit.url = $"http://{filename}";
            LearningExperienceEngine.EventManager.ActivateObject(annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(action);
            activityManager.SaveData();

            Close();
        }

        private void StartRecording()
        {
            tiltInstance.ClearScene();
            tiltSnapshot = null;
            isRecording = true;

            isPlaying = false;
            isPaused = false;

            UpdateUI();
            tiltInstance.SetViewOnly(false);
        }

        private void StopRecording()
        {
            if (isRecording)
                tiltSnapshot = tiltInstance.GetNewSnapshot();

            isRecording = false;
            UpdateUI();
            tiltInstance.SetViewOnly(true);
        }


        public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
        {
            gameObject.SetActive(true);
            this.action = action;
            annotationToEdit = annotation;

            if (Tiltbrush.Instance == null)
            {
                tiltInstance = Instantiate(tiltPrefab);
                tiltInstance.SetViewOnly(true);
            }
            else
            {
                tiltInstance = Tiltbrush.Instance;
            }

            tiltInstance.SubscribeComponent(this);

            var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
            LearningExperienceEngine.Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            tiltInstance.transform.SetParent(originT.transform);
            tiltInstance.transform.localPosition = Vector3.zero;
            tiltInstance.transform.localRotation = Quaternion.identity;
            tiltInstance.ClearScene();

            if (annotationToEdit != null && annotationToEdit.url != null)
                tiltSnapshot = tiltInstance.ImportSnapshotFromFile(GetSystemFilename(annotationToEdit.url));

            UpdateUI();
        }

        public void Close()
        {
            // when editor is closed play the spatial audio if it is exist
            if (annotationToEdit != null && GameObject.Find(annotationToEdit.poi).GetComponentInChildren<AudioPlayer>() != null)
            {
                GameObject.Find(annotationToEdit.poi).GetComponentInChildren<AudioPlayer>().PlayAudio();
            }

            StopRecording();
            tiltSnapshot = null;
            action = null;
            annotationToEdit = null;
            isPlaying = false;
            isRecording = false;
            isPaused = false;

            tiltInstance.UnsubscribeComponent(this);

            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private string GetSystemFilename(string name)
        {
            const string httpPrefix = "http://";
            return !name.StartsWith(httpPrefix)
                ? Path.Combine(Application.persistentDataPath, name)
                : Path.Combine(activityManager.ActivityPath,
                    Path.GetFileName(name.Remove(0, httpPrefix.Length)));
        }

    }
}