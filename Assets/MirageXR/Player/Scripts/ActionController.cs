
using MirageXR;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public LearningExperienceEngine.Place Content { get; private set; }

    public void Initialize(LearningExperienceEngine.Place content)
    {
        Content = content;
    }
}