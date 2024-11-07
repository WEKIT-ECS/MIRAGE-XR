using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class GridSettings : MonoBehaviour
{
    private static GridManager gridManager => RootObject.Instance.GridManager;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private Toggle _activateGridToggle;
    [SerializeField] private Toggle _snapToGridToggle;
    [SerializeField] private Toggle _showOriginalObjectToggle;
    [SerializeField] private Toggle _useObjectCenterToggle;
    [SerializeField] private Dropdown _dropdownGridStep;
    [SerializeField] private Dropdown _dropdownAngleStep;
    [SerializeField] private Dropdown _dropdownScaleStep;

    public void Start()
    {
        _dropdownGridStep.ClearOptions();
        _dropdownGridStep.AddOptions(gridManager.optionsCellSize);
        _dropdownAngleStep.ClearOptions();
        _dropdownAngleStep.AddOptions(gridManager.optionsAngleStep);
        _dropdownScaleStep.ClearOptions();
        _dropdownScaleStep.AddOptions(gridManager.optionsScaleStep);

        _activateGridToggle.isOn = gridManager.gridEnabled;
        _snapToGridToggle.isOn = gridManager.snapEnabled;
        _showOriginalObjectToggle.isOn = gridManager.showOriginalObject;
        _useObjectCenterToggle.isOn = gridManager.useObjectCenter;

        _dropdownGridStep.SetValueWithoutNotify(gridManager.valuesCellSize.IndexOf(gridManager.cellWidth));
        _dropdownAngleStep.SetValueWithoutNotify(gridManager.valuesAngleStep.IndexOf(gridManager.angleStep));
        _dropdownScaleStep.SetValueWithoutNotify(gridManager.valuesScaleStep.IndexOf(gridManager.scaleStep));

        _activateGridToggle.onValueChanged.AddListener(OnActivateGridToggleValueChanged);
        _snapToGridToggle.onValueChanged.AddListener(OnSnapToGridToggleValueChanged);
        _showOriginalObjectToggle.onValueChanged.AddListener(OnShowOriginalObjectValueChanged);
        _useObjectCenterToggle.onValueChanged.AddListener(OnUseObjectCenterValueChanged);
        _dropdownGridStep.onValueChanged.AddListener(OnItemGridStepChanged);
        _dropdownAngleStep.onValueChanged.AddListener(OnItemAngleStepChanged);
        _dropdownScaleStep.onValueChanged.AddListener(OnItemScaleStepChanged);

        EventManager.OnHideActivitySelectionMenu += Hide;
        EventManager.OnEditorLoaded += Hide;

        Hide();
    }

    public void Show(Pose pose)
    {
        transform.position = pose.position;
        transform.rotation = pose.rotation;
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }

    private void OnDestroy()
    {
        EventManager.OnHideActivitySelectionMenu -= Hide;
        EventManager.OnEditorLoaded -= Hide;
    }

    private void OnSnapToGridToggleValueChanged(bool value)
    {
        if (value)
        {
            gridManager.EnableSnapToGrid();
        }
        else
        {
            gridManager.DisableSnapToGrid();
        }
    }

    private void OnShowOriginalObjectValueChanged(bool value)
    {
        gridManager.SetShowOriginalObject(value);
    }

    private void OnUseObjectCenterValueChanged(bool value)
    {
        gridManager.SetUseObjectCenter(value);
    }

    private void OnActivateGridToggleValueChanged(bool value)
    {
        if (value)
        {
            gridManager.EnableGrid();
        }
        else
        {
            gridManager.DisableGrid();
        }
    }

    private void OnItemGridStepChanged(int item)
    {
        gridManager.SetCellWidth(gridManager.valuesCellSize[item]);
    }

    private void OnItemAngleStepChanged(int item)
    {
        gridManager.SetAngleStep(gridManager.valuesAngleStep[item]);
    }

    private void OnItemScaleStepChanged(int item)
    {
        gridManager.SetScaleStep(gridManager.valuesScaleStep[item]);
    }
}
