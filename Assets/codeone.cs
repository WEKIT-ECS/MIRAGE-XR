using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class codeone : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField; // Reference to the InputField component
     // Reference to the Button component

    private string text;           // Variable to store the saved input

    

    public string StoreInput()
    {
        text = inputField.text;
        return text;
        Debug.Log("Saved Input: " + text);
    }
  

}
