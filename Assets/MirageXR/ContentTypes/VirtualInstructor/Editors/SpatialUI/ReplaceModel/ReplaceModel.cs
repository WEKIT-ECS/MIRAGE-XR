using UnityEngine;
using UnityEngine.UI;

public class ReplaceModel : MonoBehaviour
{
    [SerializeField] private ToggleGroup replaceModelToggle; 
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject characterChip;
    [SerializeField] private GameObject addCharacter;
    [SerializeField] private Button addNewCharacter;


    void Start()
    {
        // We have to check here what character models are existing and nodeify a Load-character methode how many 
        // CharacterChips to add to the UI
        LoadCharacter(false); 
        addNewCharacter.onClick.AddListener(() => addCharacter.SetActive(true));
    }
    private void LoadCharacter(bool b)
    {
        if (b)
        {
            Debug.Log("B ist True");
            // here we need to add the characterChip to the contentPanel and we need to remove the exiting tutorial 
        }
        else
        {
            Debug.Log("B ist false");
            addNewCharacter.onClick.AddListener(() => addCharacter.SetActive(true));
        }
    }
}
