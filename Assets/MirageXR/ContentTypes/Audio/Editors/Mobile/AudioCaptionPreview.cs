using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioCaptionPreview : MonoBehaviour
{
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnEditCaption;
    [SerializeField] private TMP_Text _captionTextPreview;
    [SerializeField] private AudioCaptionGenerator _captionGenerator;

    [SerializeField] private GameObject _panelMain;
    [SerializeField] private GameObject _panelCaptionPreview;
    [SerializeField] private GameObject _panelCaptionEdit;
    [SerializeField] private GameObject _generateCaption;

    //[SerializeField] private float delayBeforeExecution = 0.0f; // Delay duration in seconds

    void Start()
    {
        _generateCaption.SetActive(true);
        _panelCaptionEdit.SetActive(false);
        //StartCoroutine(ActivateAndDelay());
        _btnBack.onClick.AddListener(OnClickBack);
        _btnEditCaption.onClick.AddListener(OnClickCaptionEdit);
        _captionTextPreview.text = _captionGenerator.GeneratedCaption();
    }

    /*IEnumerator ActivateAndDelay()
    {
        

        // Wait for the specified delay duration
        yield return new WaitForSeconds(delayBeforeExecution);
        // Execute the rest of the code after the delay
        
    }*/

    private void OnClickBack()
    {
        _panelCaptionPreview.SetActive(false);
        _panelMain.SetActive(true);
    }

    private void OnClickCaptionEdit()
    {
        _panelCaptionEdit.SetActive(true);
        _panelCaptionPreview.SetActive(false);
    }

    public string Captions() 
    {
        return _captionTextPreview.text;
    }
}
