using GLTFast;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public class GLTFDeferAgent : MonoBehaviour
    {
        public IDeferAgent GetGLTFastDeferAgent()
        {
            return GetComponent<IDeferAgent>();
        }
    }
}
