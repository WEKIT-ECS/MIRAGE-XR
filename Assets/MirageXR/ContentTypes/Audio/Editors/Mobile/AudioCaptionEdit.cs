using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioCaptionEdit : MonoBehaviour
{
    [SerializeField] private Button _btnBackPreview;
    [SerializeField] private Button _btnDoneEditing;
    [SerializeField] private TMP_InputField _captionEditText;

    [SerializeField] private GameObject _panelMain;
    [SerializeField] private GameObject _panelCaptionEdit;
    [SerializeField] private GameObject _panelCaptionPreview;

    [SerializeField] private AudioCaptionPreview audioCaptionPreview;
    private string text = string.Empty;

    // Start is called before the first frame update
    private void Start()
    {
        // Ensure all objects are properly assigned in the Unity Inspector
        if (_panelCaptionPreview == null )
        {
            Debug.LogError("_panelCaptionPreview");
            return; // Early exit to prevent accessing null references
        }if (_panelMain == null )
        {
            Debug.LogError("_panelMain");
            return; // Early exit to prevent accessing null references
        }if (_btnBackPreview == null )
        {
            Debug.LogError("_btnBackPreview.");
            return; // Early exit to prevent accessing null references
        }
        if (_btnDoneEditing == null )
        {
        Debug.LogError("_btnDoneEditing.");
            return; // Early exit to prevent accessing null references
        }
    if (_captionEditText == null )
        {
            Debug.LogError("_captionEditText");
            return; // Early exit to prevent accessing null references
        }if ( audioCaptionPreview == null)
        {
            Debug.LogError("audioCaptionPreview.");
            return; // Early exit to prevent accessing null references
        }

        _panelCaptionPreview.SetActive(false);
        _panelMain.SetActive(false);

        _btnBackPreview.onClick.AddListener(OnClickBackPreview);
        _btnDoneEditing.onClick.AddListener(doneEditing);

        // Safely assigning text, with a fallback option
        _captionEditText.text = audioCaptionPreview.Captions();
    }

    // Update is called once per frame
    private void OnClickBackPreview()
    {   
        _panelCaptionEdit.SetActive(false);
        _panelCaptionPreview.SetActive(true);
        
    }    

    private void doneEditing()
    {
        _panelCaptionEdit.SetActive(false);
        _panelMain.SetActive(true);
    }

    public string EditedCaption()
    {
        if (_captionEditText != null) 
        {
            text = _captionEditText.text;
        }
        else
        {
            Debug.LogError("_captionEditText is null. Cannot retrieve text.");
            text = ""; // Fallback or default text
        }
        return text; 
    }
}
