using UnityEngine;
using UnityEngine.UI;

public class DialogTest : MonoBehaviour
{
    [SerializeField] private Dialog _dialog;
    [SerializeField] private Button _buttonMiddle;
    [SerializeField] private Button _buttonMiddleMultiline;
    [SerializeField] private Button _buttonBottom;
    [SerializeField] private Toggle _toggleCanBeClosedByOutTap;

    private void Start()
    {
        _buttonBottom.onClick.AddListener(ShowBottomDialog);
        _buttonMiddle.onClick.AddListener(ShowMiddleDialog);
        _buttonMiddleMultiline.onClick.AddListener(ShowMiddleMultilineDialog);
    }

    private void ShowBottomDialog()
    {
        _dialog.ShowBottom(
        "Bottom Dialog Test!",
        _toggleCanBeClosedByOutTap.isOn,
        ("Item 1", () => Debug.Log("Item 1 - click!"), false),
        ("Item 2", () => Debug.Log("Item 2 - click!"), false),
        ("Item 3", () => Debug.Log("Item 3 - click!"), true),
        ("Item 4", () => Debug.Log("Item 4 - click!"), true));
    }

    private void ShowMiddleDialog()
    {
        _dialog.ShowMiddle(
            "Middle Dialog Test!",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
            "Left", () => Debug.Log("Left - click!"),
            "Right", () => Debug.Log("Right - click!"),
            _toggleCanBeClosedByOutTap.isOn);
    }

    private void ShowMiddleMultilineDialog()
    {
        _dialog.ShowMiddleMultiline(
            "Middle Multiline Dialog Test!",
            _toggleCanBeClosedByOutTap.isOn,
            ("Item 1", () => Debug.Log("Item 1 - click!"), false),
            ("Item 2", () => Debug.Log("Item 2 - click!"), false),
            ("Item 3", () => Debug.Log("Item 3 - click!"), true));
    }
}
