namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// Defines an interface for handling rotation input.
    /// This interface provides methods to detect rotation input and retrieve the corresponding rotation amount.
    /// Implementers of this interface can define custom logic for how rotation input is detected and calculated.
    /// </summary>
    public interface IAvatarRotatorInput
    {
        /// <summary>
        /// Determines if rotation input is currently being detected.
        /// </summary>
        /// <returns>A boolean indicating whether rotation input is active.</returns>
        bool IsInputDetected();

        /// <summary>
        /// Calculates and returns the amount of rotation based on the input.
        /// This method should provide the rotation value to apply based on the detected input.
        /// </summary>
        /// <returns>The amount of rotation as a float.</returns>
        float GetRotationAmount();
    }
}
