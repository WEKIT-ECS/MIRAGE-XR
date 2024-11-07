using UnityEngine;
using UnityEngine.UI;

public class ReplaceModel : MonoBehaviour
{
    [SerializeField] private Button addNewCharacter;
    [SerializeField] private GameObject replaceCharacter;

    [SerializeField] private Button close;
    [SerializeField] private GameObject replaceModelPanel;


    void Start()
    {
        addNewCharacter.onClick.AddListener(() => replaceCharacter.SetActive(true));
        close.onClick.AddListener(() => replaceModelPanel.SetActive(false));
    }
    
}
