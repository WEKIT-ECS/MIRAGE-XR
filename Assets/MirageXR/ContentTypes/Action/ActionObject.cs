using UnityEngine;

[CreateAssetMenu(fileName = "ActionObject", menuName = "ScriptableObjects/Spawn Action Object", order = 1)]
public class ActionObject : ScriptableObject
{
    public Sprite sprite;
    public string label;
    public string prefabName;
}
