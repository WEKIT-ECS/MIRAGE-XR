using MirageXR;
using UnityEngine;

[CreateAssetMenu(fileName = "LibraryObject", menuName = "ScriptableObjects/Spawn Library Object", order = 5)]
public class LibraryObject : ScriptableObject
{
    public Sprite sprite;
    public string label;
    public string prefabName;
    public ModelLibraryManager.ModelLibraryCategory category;
}