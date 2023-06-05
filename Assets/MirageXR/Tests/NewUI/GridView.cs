using System;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridView : PopupBase
{
    private static GridManager gridManager => RootObject.Instance.gridManager;

    public class IntHolder : ObjectHolder<int> { }

    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _activateGridToggle;
    [SerializeField] private Toggle _snapToGridToggle;
    [SerializeField] private GameObject _templatePrefab;
    [SerializeField] private ClampedScrollRect _clampedScrollGridStep;
    [SerializeField] private ClampedScrollRect _clampedScrollAngleStep;
    [SerializeField] private ClampedScrollRect _clampedScrollScaleStep;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        InitClampedScrollRect(_clampedScrollGridStep, _templatePrefab, 50, "cm");
        InitClampedScrollRect(_clampedScrollAngleStep, _templatePrefab, 50, "°");
        InitClampedScrollRect(_clampedScrollScaleStep, _templatePrefab, 50, "%");

        _activateGridToggle.isOn = gridManager.gridEnabled;
        _snapToGridToggle.isOn = gridManager.snapEnabled;

        _clampedScrollGridStep.currentItemIndex = ((int)gridManager.cellWidth / 5) - 1;
        _clampedScrollAngleStep.currentItemIndex = ((int)gridManager.angleStep / 5) - 1;
        _clampedScrollScaleStep.currentItemIndex = ((int)gridManager.scaleStep / 5) - 1;

        _btnClose.onClick.AddListener(Close);
        _activateGridToggle.onValueChanged.AddListener(OnActivateGridToggleValueChanged);
        _snapToGridToggle.onValueChanged.AddListener(OnSnapToGridToggleValueChanged);
        _clampedScrollGridStep.onItemChanged.AddListener(OnItemGridStepChanged);
        _clampedScrollAngleStep.onItemChanged.AddListener(OnItemAngleStepChanged);
        _clampedScrollScaleStep.onItemChanged.AddListener(OnItemScaleStepChanged);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, int maxCount, string text)
    {
        for (int i = 5; i <= maxCount; i += 5)
        {
            var obj = Instantiate(templatePrefab, clampedScrollRect.content, false);
            obj.name = i.ToString();
            obj.SetActive(true);
            obj.AddComponent<IntHolder>().item = i;
            obj.GetComponentInChildren<TMP_Text>().text = $"{i} {text}";
        }
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

    private void OnItemGridStepChanged(Component item)
    {
        var value = (float)item.GetComponent<IntHolder>().item;
        gridManager.SetCellWidth(value);
    }

    private void OnItemAngleStepChanged(Component item)
    {
        var value = (float)item.GetComponent<IntHolder>().item;
        gridManager.SetAngleStep(value);
    }

    private void OnItemScaleStepChanged(Component item)
    {
        var value = (float)item.GetComponent<IntHolder>().item;
        gridManager.SetScaleStep(value);
    }
}
