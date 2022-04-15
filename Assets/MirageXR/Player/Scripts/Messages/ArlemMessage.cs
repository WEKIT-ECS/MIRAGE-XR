using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class ArlemMessage
    {
        /// <summary>
        /// Messages received from Arlem activity files.
        /// </summary>
        /// <param name="target">Target user id.</param>
        /// <param name="message">Message content.</param>
        public ArlemMessage (string target, string message)
        {
            try
            {
                if (string.IsNullOrEmpty (target))
                    throw new ArgumentException ("Message target not set.");

                if (string.IsNullOrEmpty (message))
                    throw new ArgumentException ("Message text not set.");

                // If the message targeted for us...
                if (target == "all" || target == WorkplaceManager.GetUser())
                {
                    SpeakMessage(message);
                }
            }
            catch (Exception e)
            {
                Debug.Log (e);
                throw;
            }
        }

        private async void SpeakMessage(string message)
        {
            await Task.Delay(1500);
            // Send the message
            Maggie.Speak(message);
        }
    }
}