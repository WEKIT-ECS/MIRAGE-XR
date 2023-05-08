using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;
using UnityEngine.UI;

public class LoadView : MonoBehaviour
{
    public static LoadView Instance { get; private set; }

    [SerializeField] private Image _background;
    [SerializeField] private Image _circle;

    private int _siblingIndex;

    private void Awake()
    {
        if (Instance != null)
        {
            AppLog.LogError($"{nameof(LoadView)} must only be a single copy!");
            return;
        }

        Instance = this;

        _siblingIndex = transform.GetSiblingIndex();
        Hide();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void Show(bool dontCoverPopups = false)
    {
        if (dontCoverPopups)
        {
            var siblingIndex = PopupsViewer.Instance.transform.GetSiblingIndex();
            transform.SetSiblingIndex(siblingIndex);
        }

        _background.gameObject.SetActive(true);
        _circle.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_siblingIndex != transform.GetSiblingIndex())
        {
            transform.SetSiblingIndex(_siblingIndex);
        }

        _background.gameObject.SetActive(false);
        _circle.gameObject.SetActive(false);
    }
}