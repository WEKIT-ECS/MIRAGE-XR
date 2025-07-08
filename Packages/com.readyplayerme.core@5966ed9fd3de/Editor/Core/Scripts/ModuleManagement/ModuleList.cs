using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;

namespace ReadyPlayerMe.Core.Editor
{
    /// <summary>
    ///     Class <c>ModuleList</c> is a static class that can be referenced to get the latest module version.
    /// </summary>
    public static class ModuleList
    {
        public static ModuleInfo Core = new ModuleInfo
        {
            name = "com.readyplayerme.core",
            gitUrl = "https://github.com/readyplayerme/rpm-unity-sdk-core.git",
            branch = "",
            version = "5.0.0"
        };

        /// <summary>
        ///     A static list of all the required modules represented in an array of <c>ModuleInfo</c>.
        /// </summary>
        public static readonly ModuleInfo[] Modules =
        {
            new ModuleInfo
            {
                name = "com.readyplayerme.webview",
                gitUrl = "https://github.com/readyplayerme/rpm-unity-sdk-webview.git",
                branch = "",
                version = "2.1.3"
            }
        };

        /// <summary>
        ///     Unity Module that adds support for gltf files that use DracoCompression.
        /// </summary>
        public static ModuleInfo DracoCompression = new ModuleInfo
        {
            name = "com.atteneder.draco",
            gitUrl = "https://github.com/atteneder/DracoUnity.git",
            branch = "",
            version = "4.1.0"
        };

        /// <summary>
        ///     Get installed modules from Modules list.
        /// </summary>
        /// <returns>A <see cref="Dictionary"/> of installed Unity Module information in the format of <c>Dictionary&lt;string: name, string: version&gt;</c>. </returns>
        public static Dictionary<string, string> GetInstalledModuleVersionDictionary()
        {
            PackageInfo[] packageList = ModuleInstaller.GetPackageList();

            var installedModules = new Dictionary<string, string>();
            installedModules.Add(Core.name, Core.version);

            foreach (ModuleInfo module in Modules)
            {
                if (packageList.Any(x => x.name == module.name))
                {
                    installedModules.Add(module.name, module.version);
                }
            }

            if (packageList.Any(x => x.name == DracoCompression.name))
            {
                installedModules.Add(DracoCompression.name, DracoCompression.version);
            }

            return installedModules;
        }
    }
}
