using UnityEngine;

[CreateAssetMenu(fileName = "VfxObject", menuName = "ScriptableObjects/Spawn Vfx Object", order = 2)]
public class VfxObject : ScriptableObject
{
    public Sprite sprite;
    public string label;
    public string prefabName;
}
