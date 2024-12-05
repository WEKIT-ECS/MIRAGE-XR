using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleRotateCheckmark : MonoBehaviour
{
    [SerializeField] private RectTransform checkmark;
    [SerializeField] private GameObject activatedObject;
    [SerializeField] [Range(0, 360)] private float isOnCheckmarkRotation = 0f;
    [SerializeField] [Range(0, 360)] private float isOffCheckmarkRotation = 180f;

    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        UpdateState(_toggle.isOn);
    }

    private void OnToggleValueChanged(bool value)
    {
        UpdateState(value);
    }

    private void UpdateState(bool value)
    {
        var rotation = checkmark.localRotation.eulerAngles;
        checkmark.localEulerAngles = new Vector3(rotation.x, rotation.y, value ? isOnCheckmarkRotation : isOffCheckmarkRotation); 
        activatedObject?.SetActive(value);
    }
}
