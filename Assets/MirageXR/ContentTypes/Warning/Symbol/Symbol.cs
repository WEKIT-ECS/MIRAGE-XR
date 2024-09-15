using i5.Toolkit.Core.VerboseLogging;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Class for 2D symbol prefabs.
    /// </summary>
    public class Symbol : MirageXRPrefab
    {
        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            // Set scale, if defined in the action step configuration.
            if (!obj.scale.Equals(0))
                transform.localScale = new Vector3(obj.scale, obj.scale, obj.scale) * LearningExperienceEngine.WorkplaceManager.ScalingFactor;

            // If scaling is not set, default to 5 cm symbols.
            else
                transform.localScale = new Vector3(0.05f, 0.05f, 0.05f) * LearningExperienceEngine.WorkplaceManager.ScalingFactor;

            // Try to fetch the symbol sprite from the resources.
            var symbol = Resources.Load<Sprite>(obj.predicate);

            // If symbol couldn't be found, terminate initialization.
            if (symbol == null)
            {
                Debug.LogWarning("Symbol couldn't be found. " + obj.predicate);
                return false;
            }

            // Set the displayed sprite to the one just loaded.
            GetComponent<SpriteRenderer>().sprite = symbol;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(400 * transform.localScale.x, 400 * transform.localScale.y, 0.05f);

            // Check if we should animate the symbol.
            if (obj.option.StartsWith("animation:"))
            {
                // Get the animation reference.
                var animationId = obj.option.Split(':')[1];

                // Handle animations.
                switch (animationId)
                {
                    // If the animation type is unknown, terminate the initialization.
                    default:
                        Debug.LogWarning("Unknown animation type. " + animationId);
                        return false;

                    // For rotation types.
                    case "rotate-ccw":
                    case "rotate-cw":
                        // Enable the rotation machine component.
                        var rotationMachine = GetComponent<RotationMachine>();
                        rotationMachine.enabled = true;

                        // Set rotation axis.
                        rotationMachine.ActiveAxis = RotationMachine.Axis.Z;

                        // Set rotation direction.
                        if (animationId == "rotate-ccw")
                            rotationMachine.ActiveDirection = RotationMachine.Direction.CCW;

                        else
                            rotationMachine.ActiveDirection = RotationMachine.Direction.CW;
                        break;
                }
            }

            if (!obj.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(obj))
                    return false;
            }

            else
            {
                gameObject.AddComponent<Billboard>();
            }

            // If all went well, return true.
            return true;
        }
    }
}