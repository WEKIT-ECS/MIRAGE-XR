using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class SelectObjectPannelHandler : MonoBehaviour
{

    [SerializeField] private GameObject ObjectSlot;
    [SerializeField] private GameObject ob;

    private UnityEngine.UI.Image icon;
    private Sprite Sprite;
    private Texture2D tex;


    /*
    void Start()
    {
        //tex = AssetPreview.GetMiniThumbnail(ob);

        //ObjectSlot.GetComponentInChildren<RawImage>().texture = tex;
    }
    */
}
