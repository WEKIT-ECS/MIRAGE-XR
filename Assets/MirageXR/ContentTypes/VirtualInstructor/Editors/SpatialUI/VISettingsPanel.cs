using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VISettingsPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button close;
    [SerializeField] private Button rename;
    [SerializeField] private Button deleteStep;
    [Header("References to menu")]
    [SerializeField] private TMP_Text heading;
    void Start()
    {
        close.onClick.AddListener(()=> this.gameObject.SetActive(false));
        rename.onClick.AddListener(RenameVi);
        deleteStep.onClick.AddListener(DeleteVi);
    }

    private void RenameVi()
    {
        throw new NotImplementedException();
    }

    private void DeleteVi()
    {
        throw new NotImplementedException(); 
    }
 
}
