using UnityEngine;

namespace MirageXR
{
    public class AvatarAudioController : AvatarBaseController
    {
        public void PlayAudio(AudioClip clip)
        {
            if (AvatarRefs == null)
            {
                Debug.LogWarning("Could not find avatar references", this);
            }
            else if (AvatarRefs.Speaker == null)
            {
                Debug.LogWarning("Speaker for avatar is not set and could not be found.", this);
            }

            AvatarRefs.Speaker.PlayOneShot(clip);
        }
    }
}
