using System;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This class holds all the properties necessary for failure events.
    /// </summary>
    public class FailureEventArgs : EventArgs
    {
        public string Url { get; set; }
        public string Message { get; set; }
        public FailureType Type { get; set; }
    }

    /// <summary>
    /// This class holds all the properties necessary for progress change events.
    /// </summary>
    public class ProgressChangeEventArgs : EventArgs
    {
        public string Url { get; set; }
        public float Progress { get; set; }
        public string Operation { get; set; }
    }

    /// <summary>
    /// This class holds all the properties necessary for completion events.
    /// </summary>
    public class CompletionEventArgs : EventArgs
    {
        public string Url { get; set; }

        public GameObject Avatar { get; set; }

        public AvatarMetadata Metadata { get; set; }
    }
}
