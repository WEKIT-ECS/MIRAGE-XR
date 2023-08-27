using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ImageCaptionEditView : MonoBehaviour
{
    [SerializeField] private Button _btnDoneCaption;
    [SerializeField] private TMP_InputField _textCaption;
    string _caption;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnClickCaption()
    {
        string _caption = _textCaption.text;
        Debug.Log("The caption entered is " + _caption);
    }

}
