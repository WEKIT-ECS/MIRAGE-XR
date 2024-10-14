using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListItemBoxPicker : MonoBehaviour
{
    [SerializeField] private TMP_Text _SetOptionText;
    [SerializeField] private Button _CloseButton;
    [SerializeField] private ToggleGroup _toggleGroup;
    private Toggle[] _toggles;
    public delegate void ToggleSelectedDelegate(Toggle toggle, ListItemBoxPicker sender);
    public event ToggleSelectedDelegate OnToggleSelected;

    void Start()
    {
        _CloseButton.onClick.AddListener(Close);
        _toggles = GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in _toggles)
        {
            _toggleGroup.RegisterToggle(toggle);
            toggle.onValueChanged.AddListener(delegate { OnToggleChanged(toggle); });
        }
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            _toggleGroup.NotifyToggleOn(changedToggle);
            UnityEngine.Debug.Log("Das ist der Fall.");
            UpdateField(changedToggle.name);
            OnToggleSelected?.Invoke(changedToggle, this);
        }
    }

    void UpdateField(string str)
    {
        _SetOptionText.text = str;
        Close(); 
    }
    
    private void Close()
    {
        gameObject.SetActive(false);
    }
}