using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridView : PopupBase
{
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

        InitClampedScrollRect(_clampedScrollGridStep, _templatePrefab, 15, "cm");
        InitClampedScrollRect(_clampedScrollAngleStep, _templatePrefab, 15, "°");
        InitClampedScrollRect(_clampedScrollScaleStep, _templatePrefab, 15, "%");


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
            obj.GetComponentInChildren<TMP_Text>().text = i.ToString() + " " + text;
        }
    }

    private void OnSnapToGridToggleValueChanged(bool arg0)
    {
        // TODO
    }

    private void OnActivateGridToggleValueChanged(bool arg0)
    {
        // TODO
    }

    private void OnItemGridStepChanged(Component item)
    {
        // TODO
    }

    private void OnItemAngleStepChanged(Component item)
    {
        // TODO
    }

    private void OnItemScaleStepChanged(Component item)
    {
        // TODO
    }
}
