using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Plays back various system sounds.
    /// </summary>
    public class SystemSounds : MonoBehaviour
    {
        // Audio source object that plays out the sounds.
        private static AudioSource _speaker;

        [Tooltip("Sound played when IoT trigger triggers.")]
        public AudioClip IotTrigger;

        private void Start ()
        {
            _speaker = GetComponent<AudioSource> ();
        }

        /// <summary>
        /// Play IoT trigger sound.
        /// </summary>
        private void PlayIotTrigger ()
        {
            _speaker.clip = IotTrigger;
            _speaker.Play ();
        }
    }
}