using System;

namespace ReadyPlayerMe.Core
{
    /// <summary>
    /// This is a custom exception class used for exceptions in the Avatar Loader namespace.
    /// </summary>
    public class CustomException : Exception
    {
        public readonly FailureType FailureType;

        /// <summary>
        /// This method is used to handle custom exceptions.
        /// </summary>
        /// <param name="failureType">Custom failure type.</param>
        /// <param name="message">Message that describes the exception.</param>
        public CustomException(FailureType failureType, string message) : base(message)
        {
            FailureType = failureType;
        }
    }
}
