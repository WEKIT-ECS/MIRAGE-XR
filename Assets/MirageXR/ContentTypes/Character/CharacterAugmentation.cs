using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class CharacterAugmentation : MonoBehaviour
    {
        [SerializeField] private GameObject pointCont;

        [SerializeField] private GameObject thumbnailContainer;

        private const float _distanceToTaskSTation = 0.6f;
        private const float _distanceFromGround = 0.6f;  // Increase to move the character down
        private const float _defaultScaleFactor = 0.8f;

        private LearningExperienceEngine.Action _action;
        private LearningExperienceEngine.ToggleObject _annotationToEdit;

        private GameObject _character;
        private string _modelname;


        private void Start()
        {
            foreach (var btn in thumbnailContainer.GetComponentsInChildren<Button>())
            {
                btn.onClick.AddListener(() => CreateCharacter(btn.name));
            }
        }

        public void SetMovementType()
        {
            if (!_character) return;
            var characterController = _character.GetComponent<CharacterController>();
            characterController.MovementType = "followpath";
            characterController.AgentReturnAtTheEnd = false;
            CreatePath();
        }

        private void CreatePath()
        {
            var destinations = new List<GameObject>();
            var taskStationPosition = TaskStationDetailMenu.Instance.ActiveTaskStation.transform;
            var spawnPosition = taskStationPosition.position + taskStationPosition.transform.forward * _distanceToTaskSTation; // Behind the task station
            var des = Instantiate(pointCont, spawnPosition - Vector3.up * _distanceFromGround, Quaternion.identity);
            des.transform.rotation *= Quaternion.Euler(0, 180, 0);
            des.GetComponent<Destination>().MyCharacter = _character.GetComponent<CharacterController>();
            des.transform.SetParent(_character.transform.parent);
            destinations.Add(des);

            // character.transform.position = des.transform.position + new Vector3(0.2f , 0, 0);
            _character.GetComponent<CharacterController>().Destinations = destinations;

            _character.gameObject.transform.localScale *= _defaultScaleFactor;
        }

        private void CreateCharacter(string modelname)
        {
            this._modelname = modelname;
            Create();
        }

        public async void Create()
        {
            if (_annotationToEdit != null)
            {
                _annotationToEdit.predicate = $"char:{_modelname}";
                LearningExperienceEngine.EventManager.DeactivateObject(_annotationToEdit);

            }
            else
            {
                var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
                LearningExperienceEngine.Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_action.id));
                GameObject originT = GameObject.Find(detectable.id);

                Vector3 spawnPosition = TaskStationDetailMenu.Instance.ActiveTaskStation.transform.position + Vector3.forward;
                Quaternion spawnRot = Quaternion.identity;

                Vector3 offset = Utilities.CalculateOffset(spawnPosition, spawnRot, originT.transform.position, originT.transform.rotation);

                _annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(_action, offset);
                _annotationToEdit.predicate = "char:" + _modelname;
            }

            LearningExperienceEngine.EventManager.ActivateObject(_annotationToEdit);
            LearningExperienceEngine.EventManager.NotifyActionModified(_action);
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();

            var characterObjectName = $"{_annotationToEdit.id}/{_annotationToEdit.poi}/{_annotationToEdit.predicate}";

            while (_character == null)
            {
                _character = GameObject.Find(characterObjectName);
                await Task.Delay(10);
            }

            SetMovementType();

            var characterController = _character.GetComponent<CharacterController>();
            characterController.AudioEditorCheck();
            characterController.MyAction = _action;

            Close();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _action = null;
            _annotationToEdit = null;

            Destroy(gameObject);
        }

        public void Open(LearningExperienceEngine.Action action, LearningExperienceEngine.ToggleObject annotation)
        {
            // character = myCharacter;
            gameObject.SetActive(true);
            this._action = action;
            _annotationToEdit = annotation;
        }
    }
}

