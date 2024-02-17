using System;
using UnityEngine.Events;

/// <summary>
/// Event for a changed text, e.g. of an input field
/// Necessary in order to access Unity events with arguments in the inspector
/// </summary>
[Serializable]
public class TextChangedEvent : UnityEvent<string>
{
}
