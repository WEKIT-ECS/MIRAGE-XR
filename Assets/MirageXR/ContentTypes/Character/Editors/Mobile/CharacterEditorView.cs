using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using CharacterController = MirageXR.CharacterController;

public class CharacterEditorView : PopupEditorBase
{
    public override LearningExperienceEngine.DataModel.ContentType editorForType => LearningExperienceEngine.DataModel.ContentType.Character;

    [SerializeField] private Destination _destinationPrefab;
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private CharacterListItem _characterListItemPrefab;
    [SerializeField] private CharacterObject[] _characterObjects;

    private string _prefabName;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
    }

    private void UpdateView()
    {
        for (int i = _contentContainer.childCount - 1; i >= 0; i--)
        {
            var child = _contentContainer.GetChild(i);
            Destroy(child);
        }

        foreach (var characterObject in _characterObjects)
        {
            var item = Instantiate(_characterListItemPrefab, _contentContainer);
            item.Init(characterObject, OnAccept);
        }
    }

    private async void SetupCharacter()
    {
        const string movementType = "followpath";

        var characterObjectName = $"{_content.id}/{_content.poi}/{_content.predicate}";
        var character = GameObject.Find(characterObjectName);   // TODO: possible NRE

        while (character == null)
        {
            character = GameObject.Find(characterObjectName);   // TODO: possible NRE
            await Task.Delay(10);
        }

        var characterController = character.GetComponent<CharacterController>();
        characterController.MovementType = movementType;
        characterController.AgentReturnAtTheEnd = false;

        var destinations = new List<GameObject>();
        var taskStationPosition = TaskStationDetailMenu.Instance.ActiveTaskStation.transform.position;
        character.transform.position = taskStationPosition;
        var destination = Instantiate(_destinationPrefab, taskStationPosition - Vector3.up, Quaternion.identity);
        destination.transform.rotation *= Quaternion.Euler(0, 180, 0);
        destination.MyCharacter = characterController;
        destination.transform.SetParent(character.transform.parent);
        destinations.Add(destination.gameObject);

        characterController.Destinations = destinations;
        characterController.AudioEditorCheck();
        characterController.MyAction = _step;
    }

    private void OnAccept(string prefabName)
    {
        _prefabName = prefabName;
        OnAccept();
    }

    protected override void OnAccept()
    {
        if (_content != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
        }

        _content.predicate = $"char:{_prefabName}";
        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();

        SetupCharacter();
        Close();
    }
}
