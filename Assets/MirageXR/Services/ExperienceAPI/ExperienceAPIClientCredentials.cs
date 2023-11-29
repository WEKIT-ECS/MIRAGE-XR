using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Scriptable object to store the client credentials for an xAPI client
    /// </summary>
    [CreateAssetMenu(fileName = "New xAPI Client", menuName = "xAPI Client", order = 0)]
    public class ExperienceAPIClientCredentials : ScriptableObject
    {
        /// <summary>
        /// The authorization token that allows the client to make requests to the server
        /// </summary>
        public string authToken;
    }
}