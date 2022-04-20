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
        private const float _distanceFromGround = 0.6f;  //Increase to move the character down
        private const float _defaultScaleFactor = 0.8f;

        private Action action;
        private ToggleObject annotationToEdit;

        private GameObject character;
        private string modelname;


        private void Start()
        {
            foreach (var btn in thumbnailContainer.GetComponentsInChildren<Button>())
            {
                btn.onClick.AddListener(() => CreateCharacter(btn.name));
            }
        }

        public void SetMovementType()
        {
            if (!character) return;
            var characterController = character.GetComponent<CharacterController>();
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
            des.transform.rotation *= Quaternion.Euler(0, 180 , 0);
            des.GetComponent<Destination>().MyCharacter = character.GetComponent<CharacterController>();
            des.transform.SetParent(character.transform.parent);
            destinations.Add(des);

           // character.transform.position = des.transform.position + new Vector3(0.2f , 0, 0);
            character.GetComponent<CharacterController>().Destinations = destinations;

            character.gameObject.transform.localScale *= _defaultScaleFactor;
        }

        private void CreateCharacter(string modelname)
        {
            this.modelname = modelname;
            Create();
        }

        public async void Create()
        {
            if (annotationToEdit != null)
            {
                annotationToEdit.predicate = $"char:{modelname}";
                EventManager.DeactivateObject(annotationToEdit);

            }
            else
            {
                var workplaceManager = RootObject.Instance.workplaceManager;
                Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
                GameObject originT = GameObject.Find(detectable.id);

                Vector3 spawnPosition =  TaskStationDetailMenu.Instance.ActiveTaskStation.transform.position + Vector3.forward;
                Quaternion spawnRot = Quaternion.identity;

                Vector3 offset = Utilities.CalculateOffset(spawnPosition, spawnRot, originT.transform.position, originT.transform.rotation);

                annotationToEdit = RootObject.Instance.augmentationManager.AddAugmentation(action, offset);
                annotationToEdit.predicate = "char:" + modelname;
            }

            EventManager.ActivateObject(annotationToEdit);
            EventManager.NotifyActionModified(action);

            var characterObjectName = $"{annotationToEdit.id}/{annotationToEdit.poi}/{annotationToEdit.predicate}";

            while (character == null)
            {
                character = GameObject.Find(characterObjectName);
                await Task.Delay(10);
            }

            SetMovementType();

            var characterController = character.GetComponent<CharacterController>();
            characterController.AudioEditorCheck();
            characterController.MyAction = action;

            Close();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            action = null;
            annotationToEdit = null;

            Destroy(gameObject);
        }

        public void Open(Action action, ToggleObject annotation)
        {
            //character = myCharacter;
            gameObject.SetActive(true);
            this.action = action;
            annotationToEdit = annotation;
        }
    }
}

