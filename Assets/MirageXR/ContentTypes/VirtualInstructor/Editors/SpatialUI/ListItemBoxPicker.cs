using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListItemBoxPicker : MonoBehaviour
{
    [SerializeField] private TMP_Text _SetOptionText;
    [SerializeField] private Button _CloseButton;
    [SerializeField] private ToggleGroup _toggleGroup; 
    private Toggle[] _toggles;
    
    
    void Start()
    {
        _CloseButton.onClick.AddListener(Close);
        _toggles = GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in _toggles)
        {
            _toggleGroup.RegisterToggle(toggle);
            toggle.onValueChanged.AddListener(delegate {
                OnToggleChanged(toggle);
            });
        }
    }

   

    void OnToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            _toggleGroup.NotifyToggleOn(changedToggle);
            UpdateField(changedToggle.name); 
        }

    }

    // Beispielmethode zum Aktualisieren eines Feldes
    void UpdateField(string str)
    {
        _SetOptionText.text = str;
        Close(); 
    }
    
    private void Close()
    {
       this.gameObject.SetActive(false);
    }

    
}
