using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.AvatarRenders
{
    public class RenderPanel : MonoBehaviour
    {
        [SerializeField] private string url = "https://models.readyplayer.me/632d65e99b4c6a4352a9b8db.glb";
        [SerializeField] private AvatarRenderSettings renderSettings;
        [SerializeField] private Image image;
        
        private void Start()
        {
            var avatarRenderLoader = new AvatarRenderLoader();
            avatarRenderLoader.OnCompleted = SetImage;
            avatarRenderLoader.LoadRender(url, renderSettings);
        }

        private void SetImage(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
            image.sprite = sprite;
            image.preserveAspect = true;
        }
    }
}
