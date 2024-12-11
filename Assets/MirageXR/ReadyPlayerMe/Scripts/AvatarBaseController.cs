using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public abstract class AvatarBaseController : MonoBehaviour
    {
        protected AvatarReferences _avatarRefs;

        public void SetReferences(AvatarReferences avatarReferences)
        {
            _avatarRefs = avatarReferences;
        }

        protected virtual void Start()
        {
            if (_avatarRefs == null)
            {
                _avatarRefs = GetComponentInParent<AvatarReferences>();
            }
        }
    }
}
