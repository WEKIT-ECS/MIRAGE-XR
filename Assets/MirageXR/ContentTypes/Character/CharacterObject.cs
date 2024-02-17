using UnityEngine;

[CreateAssetMenu(fileName = "CharacterObject", menuName = "ScriptableObjects/Spawn Character Object", order = 3)]
public class CharacterObject : ScriptableObject
{
    public Sprite sprite;
    public string label;
    public string prefabName;
}