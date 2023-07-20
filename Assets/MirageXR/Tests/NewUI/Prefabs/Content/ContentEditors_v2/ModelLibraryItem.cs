using System;
using UnityEngine;
using UnityEngine.UI;

public class ModelLibraryItem : MonoBehaviour
{
    [SerializeField] private Button _buttonSelectLibrary;
    private Action<ModelLibraryItem> _acceptAction;

    public void Init(Action<ModelLibraryItem> acceptAction)
    {
        _acceptAction = acceptAction;
        _buttonSelectLibrary.onClick.AddListener(OnSelectLibrary);
    }

    private void OnSelectLibrary()
    {
        _acceptAction(this);
    }
}
