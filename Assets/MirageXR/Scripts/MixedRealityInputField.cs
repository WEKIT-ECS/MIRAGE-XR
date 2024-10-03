using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

public class MixedRealityInputField : MonoBehaviour
{
    [SerializeField] private Text _textComponent;

    private Selectable _selectable;

    private string _text;

    public TextChangedEvent OnTextChanged;

    public delegate void OnTextChangedDelegate(string textw3);

    public event OnTextChangedDelegate OnTextUpdated;

    public bool Interactable
    {
        get
        {
#if !(UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS)
            EnsureButtonReference();
#endif
            return _selectable != null && _selectable.interactable;
        }
        set
        {
#if !(UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS)
            EnsureButtonReference();
#endif
            if (_selectable) _selectable.interactable = value;
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;

            _text = value;
            _textComponent.text = _text;
            OnTextChanged?.Invoke(_text);
            OnTextUpdated?.Invoke(_text);
#if UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS
            if (_selectable is InputField inputField) inputField.text = _text;
#endif
        }
    }

    private void EnsureButtonReference()
    {
        if (_selectable != null)
        {
            return;
        }

        _selectable = gameObject.GetComponent<Button>();
        if (_selectable == null)
        {
            _selectable = gameObject.AddComponent<Button>();
        }
    }

    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_VISIONOS
        ReplaceInput();
#endif
    }

    private void Start()
    {
        EnsureButtonReference();
        if (_selectable is Button btn)
        {
            btn.onClick.AddListener(OnTextFieldButtonPressed);
        }
    }

    private void ReplaceInput()
    {
        var unityInput = gameObject.AddComponent<InputField>();
        if (!unityInput)
        {
            var selectable = gameObject.GetComponent<Selectable>();
            var colors = selectable.colors;
            DestroyImmediate(selectable);
            unityInput = gameObject.AddComponent<InputField>();
            unityInput.colors = colors;
            _selectable = unityInput;
        }
        var oldText = _textComponent.text;
        unityInput.textComponent = _textComponent;
        unityInput.lineType = InputField.LineType.MultiLineNewline;
        unityInput.text = oldText;
        unityInput.onValueChanged.AddListener(text => Text = text);
        var pressableButton = gameObject.GetComponent<PressableButton>();
        if (pressableButton) pressableButton.enabled = false;
        var touchableUI = gameObject.GetComponent<NearInteractionTouchableUnityUI>();
        if (touchableUI) touchableUI.enabled = false;
        enabled = false;
    }

    public void OnTextFieldButtonPressed()
    {
        NonNativeKeyboard.Instance.PresentKeyboard(_text);
        NonNativeKeyboard.Instance.RepositionKeyboard(transform.position - 0.05f * transform.forward);
        NonNativeKeyboard.Instance.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        NonNativeKeyboard.Instance.OnTextUpdated += TextUpdated;
        NonNativeKeyboard.Instance.OnClosed += KeyboardClosed;
    }

    private void KeyboardClosed(object sender, EventArgs e)
    {
        NonNativeKeyboard.Instance.OnTextUpdated -= TextUpdated;
        NonNativeKeyboard.Instance.OnClosed -= KeyboardClosed;
    }

    private void TextUpdated(string obj)
    {
        Text = obj;
    }
}
