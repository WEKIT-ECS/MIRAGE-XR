using System.Linq;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentListItem_v2 : MonoBehaviour
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private TMP_Text _txtType;
    [SerializeField] private TMP_Text _txtName;
    [SerializeField] private Image _imgType;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnListItem;

    private ContentListView_v2 _parentView;
    private ToggleObject _content;
    private ContentType _type;
    private int _from;
    private int _to;

    public delegate void OnAnnotationItemClickedDelegate(ToggleObject annotation);

    public event OnAnnotationItemClickedDelegate OnAnnotationItemClicked;

    private int _maxStepIndex => activityManager.ActionsOfTypeAction.Count - 1;

    public void Init(ContentListView_v2 parentView)
    {
        _parentView = parentView;
        _btnSettings.onClick.AddListener(OnSettingsPressed);
        _btnListItem.onClick.AddListener(OnListItemPressed);
    }

    public void UpdateView(ToggleObject content)
    {
        _content = content;
        _type = ContentTypeExtenstion.ParsePredicate(_content.predicate);
        _txtType.text = _content.predicate;
        _txtName.text = _type.GetName();
        _imgType.sprite = _type.GetIcon();

        var stepList = activityManager.ActionsOfTypeAction;

        _from = stepList.FindIndex(step => step.enter.activates.Any(t => t.poi == _content.poi));
        _to = stepList.FindLastIndex(step => step.enter.activates.Any(t => t.poi == _content.poi));

        if (_parentView.navigatorId == _content.poi)
        {
            TaskStationDetailMenu.Instance.NavigatorTarget = ActionListMenu.CorrectTargetObject(_content);
        }
    }

    private void OnSettingsPressed()
    {
        RootView_v2.Instance.dialog.ShowBottomMultiline("Settings", 
            ("Edit", EditContent, false),
            ("Rename", RenameContent, false),
            ($"Keep alive {_from}-{_to}", ChangeKeepAlive, false),
            ("Delete", DeleteContent, true));
        
    }

    private void OnListItemPressed()
    {
        OnAnnotationItemClicked?.Invoke(_content);
    }

    private void EditContent()
    {
        var type = ContentTypeExtenstion.ParsePredicate(_content.predicate);
        var editor = _parentView.editors.FirstOrDefault(t => t.editorForType == type);
        if (editor == null)
        {
            Debug.LogError($"there is no editor for the type {type}");
            return;
        }

        PopupsViewer.Instance.Show(editor, _parentView.currentStep, _content);
    }

    private void RenameContent()
    {
        //not implemented
    }

    private void DeleteContent()
    {
        RootView_v2.Instance.dialog.ShowMiddle("Warning!", "Are you sure you want to delete this content?",
            "Yes", () =>
            {
                RootObject.Instance.augmentationManager.DeleteAugmentation(_content);
                if (_parentView.navigatorId == _content.poi)
                {
                    TaskStationDetailMenu.Instance.NavigatorTarget = null;
                }
            }, "No", null);
    }

    private void ChangeKeepAlive()
    {
        //not implemented
    }

    private void UpdateStep()
    {
        RootObject.Instance.augmentationManager.AddAllAugmentationsBetweenSteps(_from, _to, _content, Vector3.zero);
        if (_type == ContentType.CHARACTER)
        {
            activityManager.SaveData();
        }
    }
}
