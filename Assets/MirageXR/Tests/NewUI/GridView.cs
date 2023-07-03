using System;
using System.Collections.Generic;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridView : PopupBase
{
    private static GridManager gridManager => RootObject.Instance.gridManager;

    public class IntHolder : ObjectHolder<float> { }

    [SerializeField] private Button _btnClose;
    [SerializeField] private Toggle _activateGridToggle;
    [SerializeField] private Toggle _snapToGridToggle;
    [SerializeField] private Toggle _showOriginalObjectToggle;
    [SerializeField] private Toggle _useObjectCenterToggle;
    [SerializeField] private GameObject _templatePrefab;
    [SerializeField] private ClampedScrollRect _clampedScrollGridStep;
    [SerializeField] private ClampedScrollRect _clampedScrollAngleStep;
    [SerializeField] private ClampedScrollRect _clampedScrollScaleStep;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        InitClampedScrollRect(_clampedScrollGridStep, _templatePrefab, gridManager.optionsCellSize, gridManager.valuesCellSize);
        InitClampedScrollRect(_clampedScrollAngleStep, _templatePrefab, gridManager.optionsAngleStep, gridManager.valuesAngleStep);
        InitClampedScrollRect(_clampedScrollScaleStep, _templatePrefab, gridManager.optionsScaleStep, gridManager.valuesScaleStep);

        _activateGridToggle.isOn = gridManager.gridEnabled;
        _snapToGridToggle.isOn = gridManager.snapEnabled;
        _showOriginalObjectToggle.isOn = gridManager.showOriginalObject;
        _useObjectCenterToggle.isOn = gridManager.showOriginalObject;

        _clampedScrollGridStep.currentItemIndex = gridManager.valuesCellSize.IndexOf(gridManager.cellWidth);
        _clampedScrollAngleStep.currentItemIndex = gridManager.valuesAngleStep.IndexOf(gridManager.angleStep);
        _clampedScrollScaleStep.currentItemIndex = gridManager.valuesScaleStep.IndexOf(gridManager.scaleStep);

        _btnClose.onClick.AddListener(Close);
        _activateGridToggle.onValueChanged.AddListener(OnActivateGridToggleValueChanged);
        _snapToGridToggle.onValueChanged.AddListener(OnSnapToGridToggleValueChanged);
        _showOriginalObjectToggle.onValueChanged.AddListener(OnShowOriginalObjectValueChanged);
        _useObjectCenterToggle.onValueChanged.AddListener(OnUseObjectCenterValueChanged);
        _clampedScrollGridStep.onItemChanged.AddListener(OnItemGridStepChanged);
        _clampedScrollAngleStep.onItemChanged.AddListener(OnItemAngleStepChanged);
        _clampedScrollScaleStep.onItemChanged.AddListener(OnItemScaleStepChanged);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private static void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, IReadOnlyList<string> texts, IReadOnlyList<float> values)
    {
        if (texts.Count != values.Count)
        {
            throw new ApplicationException("texts and values must have equal length");
        }

        for (int i = 0; i < texts.Count; i++)
        {
            var obj = Instantiate(templatePrefab, clampedScrollRect.content, false);
            obj.name = texts[i];
            obj.SetActive(true);
            obj.AddComponent<IntHolder>().item = values[i];
            obj.GetComponentInChildren<TMP_Text>().text = texts[i];
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

    private void OnShowOriginalObjectValueChanged(bool value)
    {
        gridManager.SetShowOriginalObject(value);
    }

    private void OnUseObjectCenterValueChanged(bool value)
    {
        gridManager.SetUseObjectCenter(value);
    }

    private void OnItemGridStepChanged(Component item)
    {
        var value = item.GetComponent<IntHolder>().item;
        gridManager.SetCellWidth(value);
    }

    private void OnItemAngleStepChanged(Component item)
    {
        var value = item.GetComponent<IntHolder>().item;
        gridManager.SetAngleStep(value);
    }

    private void OnItemScaleStepChanged(Component item)
    {
        var value = item.GetComponent<IntHolder>().item;
        gridManager.SetScaleStep(value);
    }
}
