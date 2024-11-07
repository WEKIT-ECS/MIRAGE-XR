using TMPro;
using UnityEngine;

public class InputFieldLineController : MonoBehaviour
{
	[SerializeField] private GameObject line; 
	[SerializeField] private TMP_InputField input;
    [SerializeField] private AddEditVirtualInstructor addEditVirtualInstructor;
    void Start()
    {
        line.SetActive(false);
        input.onSelect.AddListener(OnInputFieldFocused);
        input.onDeselect.AddListener(OnInputFieldUnfocused);
    }

    private void OnInputFieldUnfocused(string arg0)
    {
        addEditVirtualInstructor.UpdateName(input.text);
        line.SetActive(false);
    }

    private void OnInputFieldFocused(string arg0)
    {
        line.SetActive(true);
    }
    
}
