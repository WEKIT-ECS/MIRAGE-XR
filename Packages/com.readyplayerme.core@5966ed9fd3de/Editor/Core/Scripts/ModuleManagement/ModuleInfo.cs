using System;

namespace ReadyPlayerMe.Core.Editor
{
    /// <summary>
    ///     Structure <c>ModuleInfo</c> describes a Ready Player Me Module or Unity package.
    /// </summary>
    [Serializable]
    public struct ModuleInfo
    {
        public string name;
        public string gitUrl;
        public string branch;
        public string version;

        /// <summary>
        ///     Get the Unity package identifier.
        /// </summary>
        /// <returns>
        ///     A <c>string</c> representing the Unity packages Git Url including branch if specified. Returns module name if
        ///     gitUrl is not set.
        /// </returns>
        public string Identifier
        {
            get
            {
                if (gitUrl == string.Empty)
                {
                    return name;
                }
                // if branch not set, default to the version in ModuleList
                return gitUrl + (string.IsNullOrEmpty(branch) ? $"#v{version}" : $"#{branch}");

            }
        }
    }
}
