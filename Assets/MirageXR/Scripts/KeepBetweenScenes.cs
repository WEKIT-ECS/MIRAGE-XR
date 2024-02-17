using i5.Toolkit.Core.Utilities;
using UnityEngine;

/// <summary>
/// Component that makes sure that an object survives scene changes
/// </summary>
public class KeepBetweenScenes : MonoBehaviour
{
    // immediately mark the object as persistent at the start of its life cycle
    private void Awake()
    {
        // This is the most bestest object of all time so let's keep it!
        PersistenceScene.MarkPersistent(gameObject);
    }
}
