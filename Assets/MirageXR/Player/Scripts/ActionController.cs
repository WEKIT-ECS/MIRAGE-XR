
using MirageXR;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public Place Content { get; private set; }

    public void Initialize(Place content)
    {
        Content = content;
    }
}