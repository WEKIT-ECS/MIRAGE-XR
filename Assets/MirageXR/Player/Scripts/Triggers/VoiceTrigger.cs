using System;
using TiltBrush;
using UnityEngine;

namespace MirageXR
{
    public class VoiceTrigger : MonoBehaviour
    {
        // // Action id of the action using this trigger.
        private string _actionId;

        private void OnEnable()
        {
            EventManager.OnPlayerReset += Delete;
        }

        private void OnDisable()
        {
            EventManager.OnPlayerReset -= Delete;
        }

        private void Delete()
        {
            Destroy(gameObject);
        }

        private void Start()
        {
            // Add VoiceTrigger tag. There can be only one...
            gameObject.tag = "VoiceTrigger";
        }

        /// <summary>
        /// Add action id to voice trigger.
        /// </summary>
        /// <param name="actionId">Action id off the triggering action.</param>
        public void AttachAction(string actionId)
        {
            try
            {
                if (string.IsNullOrEmpty(actionId))
                    throw new ArgumentException("Action id not set.");

                _actionId = actionId;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Launch the trigger
        /// </summary>
        private void DoVoiceTrigger()
        {
            RootObject.Instance.activityManager.DeactivateAction(_actionId).AsAsyncVoid();
            //Maggie.Ok();
        }
    }
}