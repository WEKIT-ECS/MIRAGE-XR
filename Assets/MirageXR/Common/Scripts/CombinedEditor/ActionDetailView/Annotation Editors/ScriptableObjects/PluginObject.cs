using UnityEngine;

[CreateAssetMenu(fileName = "PluginObject", menuName = "ScriptableObjects/Spawn Plugin Object", order = 4)]
public class PluginObject : ScriptableObject
{
    public Texture2D icon;
    public GameObject pluginPrefab;
    public string id;
    public string pluginName;
    public string manifest;

    public Sprite sprite
    {
        get
        {
            var rect = new Rect(0, 0, icon.width, icon.height);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(icon, rect, pivot);
        }
    }
}