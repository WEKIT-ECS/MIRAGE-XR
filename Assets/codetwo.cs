using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class codetwo : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputFieldTwo;   // Reference to the second InputField component where we want to display the saved text
     [SerializeField]
    private Button submitButton; 
    [SerializeField]
    private TMP_Text _inputFieldTwo; 
    [SerializeField]
    private codeone inputReader;    // Reference to the InputReader script to get the saved text
private void Start()
    {
        submitButton.onClick.AddListener(DisplayText);
    }
    public void DisplayText()
    {
        string savedText = inputReader.StoreInput();
        inputFieldTwo.text = savedText;
        Debug.Log(savedText);
        _inputFieldTwo.text = savedText;
    }
}
