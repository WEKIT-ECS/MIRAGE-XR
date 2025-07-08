using System;
using System.Threading;
using System.Threading.Tasks;
using ReadyPlayerMe.AvatarCreator;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TaskExtensions = ReadyPlayerMe.AvatarCreator.TaskExtensions;

public class UserAvatarElement : MonoBehaviour
{
    [SerializeField]
    private AvatarElementButton[] buttonActions;
    public RawImage AvatarImage;
    public UnityEvent OnImageLoaded;

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Adds the listeners to the button's onClick event.
    /// </summary>
    /// <param name="avatarId">An avatar Id. This is needed to download the avatar image</param>
    /// <param name="action">An operator, that has a list of AvatarListItemAction actions, that can be listened</param>
    public async void SetupButton(string avatarId, Action<AvatarListItemAction> action)
    {
        foreach (AvatarElementButton avatarElementAction in buttonActions)
        {
            avatarElementAction.button.onClick.AddListener(() => action?.Invoke(new AvatarListItemAction()
            {
                ActionType = avatarElementAction.actionType,
                AvatarId = avatarId
            }));
        }
        
        await TaskExtensions.HandleCancellation(SetIcon(avatarId));
    }

    /// <summary>
    /// Sets the icon of the avatar component
    /// </summary>
    /// <param name="avatarId">ID of the avatar</param>
    private async Task SetIcon(string avatarId)
    {
        if (AvatarImage == null)
        {
            return;
        }
        
        var texture = await AvatarRenderHelper.GetPortrait(avatarId, cancellationTokenSource.Token);
        AvatarImage.texture = texture;
        OnImageLoaded?.Invoke();
        
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    public enum ButtonAction
    {
        Delete,
        Select,
        Customize
    };
    [Serializable]
    public struct AvatarElementButton
    {
        public Button button;
        public ButtonAction actionType;
    }
    public struct AvatarListItemAction
    {
        public ButtonAction ActionType;
        public string AvatarId;
    }
}
