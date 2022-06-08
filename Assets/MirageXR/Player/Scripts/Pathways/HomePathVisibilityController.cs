using MirageXR;
using UnityEngine;

[RequireComponent(typeof(PathSegmentsController))]
public class HomePathVisibilityController : MonoBehaviour
{
    public static HomePathVisibilityController Instance { get; private set; }

    [SerializeField] private GameObject[] hideableObjects;

    private PathSegmentsController segmentController;

    private void Start()
    {
        // create Singleton
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        segmentController = GetComponent<PathSegmentsController>();
        
        if (!PlatformManager.Instance.WorldSpaceUi)
        {
            SetVisibility(false);
            return;
        }
        
        EventManager.OnHideActivitySelectionMenu += OnHideActivitySelectionMenu;
        EventManager.OnEditorLoaded += OnHideActivitySelectionMenu;
        EventManager.OnShowActivitySelectionMenu += OnShowActivitySelectionMenu;
    }

    private void OnDestroy()
    {
        EventManager.OnHideActivitySelectionMenu -= OnHideActivitySelectionMenu;
        EventManager.OnEditorLoaded -= OnHideActivitySelectionMenu;
        EventManager.OnShowActivitySelectionMenu -= OnShowActivitySelectionMenu;
    }

    public void ToggleActivitySelectionMenu()
    {
        if (segmentController.endTransform.gameObject.activeInHierarchy)
        {
            EventManager.HideActivitySelectionMenu();
        }
        else
        {
            EventManager.ShowActivitySelectionMenu();
        }
    }

    private void OnHideActivitySelectionMenu()
    {
        SetVisibility(false);
    }

    private void OnShowActivitySelectionMenu()
    {
        SetVisibility(true);
    }

    private void SetVisibility(bool visibility)
    {
        segmentController.IsVisible = visibility;
        for (int i = 0; i < hideableObjects.Length; i++)
        {
            hideableObjects[i].SetActive(visibility);
        }
    }
}

