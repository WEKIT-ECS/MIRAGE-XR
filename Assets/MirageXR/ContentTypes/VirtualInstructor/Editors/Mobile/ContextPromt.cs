using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Represents a contextual prompt in a Unity scene.
/// </summary>
public class ContextPromt : MonoBehaviour
{
    [SerializeField] private Button editButton;

    /// <summary>
    /// The TMP_InputField variable used in the ContextPromt class.
    /// </summary>
    [SerializeField] private TMP_InputField tmpText;

    /// <summary>
    /// The text displayed on the button.
    /// </summary>
    [FormerlySerializedAs("butonText")] [SerializeField] private TMP_Text buttonText;

    /// <summary>
    /// Initializes the ContextPromt component.
    /// </summary>
    private void Start()
    {
        tmpText.enabled= false;
        editButton.onClick.AddListener(EnableText); 
    }

    /// <summary>
    /// Enables or disables the input field for editing.
    /// </summary>
    private void EnableText()
    {
        if (tmpText.enabled == false)
        {
            tmpText.enabled= true;
            buttonText.SetText("Save");
        }
        else
        {
            tmpText.enabled= false;
            buttonText.SetText("Edit text");
        }
    }
}
