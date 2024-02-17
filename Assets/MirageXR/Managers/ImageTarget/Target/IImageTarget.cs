using UnityEngine;

public interface IImageTarget
{
    string imageTargetName { get; }

    GameObject targetObject { get; }

    UnityEventImageTarget onTargetFound { get; }

    UnityEventImageTarget onTargetLost { get; }
}
