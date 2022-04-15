using UnityEngine;
using UnityEngine.UI;

public class LoadView : MonoBehaviour
{
    public static LoadView Instance { get; private set; }

    [SerializeField] private Image _background;
    [SerializeField] private Image _circle;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{nameof(LoadView)} must only be a single copy!");
            return;
        }
        
        Instance = this;
        
        Hide();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void Show()
    {
        _background.gameObject.SetActive(true);
        _circle.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _background.gameObject.SetActive(false);
        _circle.gameObject.SetActive(false);
    }
}