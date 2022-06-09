using UnityEngine;

[CreateAssetMenu(fileName = "eROBSONObject", menuName = "ScriptableObjects/Spawn eROBSON Object", order = 2)]
public class eROBSONObject : ScriptableObject
{
    public Sprite sprite;
    public string label;
    public string prefabName;
}
