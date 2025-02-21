using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MirageXR;

public class KeepAliveView : PopupBase
{
    public class IntHolder : ObjectHolder<int> { }

    [SerializeField] private GameObject _templatePrefab;
    [SerializeField] private ClampedScrollRect _clampedScrollFrom;
    [SerializeField] private ClampedScrollRect _clampedScrollTo;
    [SerializeField] private Button _btnOk;
    [SerializeField] private Button _btnCancel;

    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    private int _stepCount;
    private int _from;
    private int _to;
    private Action<int, int> _callback;

    public override void Initialization(Action<PopupBase> onClose, params object[] args) // args: _stepCount, _from, _to, _callback
    {
        base.Initialization(onClose, args);

        InitClampedScrollRect(_clampedScrollFrom, _templatePrefab, _stepCount);
        InitClampedScrollRect(_clampedScrollTo, _templatePrefab, _stepCount);

        _clampedScrollFrom.currentItemIndex = _from;
        _clampedScrollTo.currentItemIndex = _to;

        _btnOk.onClick.AddListener(OnButtonOkClicked);
        _btnCancel.onClick.AddListener(OnButtonCancelClicked);
        _clampedScrollFrom.onItemChanged.AddListener(OnItemFromChanged);
        _clampedScrollTo.onItemChanged.AddListener(OnItemToChanged);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _stepCount = (int)args[0];
            _from = (int)args[1];
            _to = (int)args[2];
            _callback = (Action<int, int>)args[3];

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void InitClampedScrollRect(ClampedScrollRect clampedScrollRect, GameObject templatePrefab, int maxCount)
    {
        for (int i = 0; i < maxCount; i++)
        {
            var obj = Instantiate(templatePrefab, clampedScrollRect.content, false);
            obj.name = i.ToString();
            obj.SetActive(true);
            obj.AddComponent<IntHolder>().item = i;
            obj.GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
        }
    }

    private void OnButtonOkClicked()
    {
        if (_from > _to)
        {
            Toast.Instance.Show("The 'From' should be less than the 'To'");
            return;
        }

        _callback?.Invoke(_from, _to);
        activityManager.SaveData();
        Close();
    }

    private void OnButtonCancelClicked()
    {
        Close();
    }

    private void OnItemFromChanged(Component item)
    {
        _from = item.GetComponent<ObjectHolder<int>>().item;
    }

    private void OnItemToChanged(Component item)
    {
        _to = item.GetComponent<ObjectHolder<int>>().item;
    }
}
