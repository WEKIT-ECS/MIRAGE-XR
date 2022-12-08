using UnityEngine;
using UnityEngine.UI;

public class TutorialItem : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private GameObject _interactableObject;

    public string id => _id;

    public Button button => _interactableObject.GetComponent<Button>();

    public Toggle toggle => _interactableObject.GetComponent<Toggle>();

    //public string pathToButton => GetPathToButton();

    //private string GetPathToButton()
    //{
    //    var parent = _interactableObject.transform;
    //    var stringBuilder = new StringBuilder();
    //    do
    //    {
    //        stringBuilder.Insert(0, $"\\{parent.name}");
    //    } while (parent != transform);

    //    return stringBuilder.ToString();
    //}
}
