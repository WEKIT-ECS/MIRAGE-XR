using System.Linq;
using System.Threading;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;

namespace ReadyPlayerMe.Core.Editor
{
    public class SampleLoader
    {
        private Sample loadedSample;

        public bool Load(string packageName, string sampleName)
        {
            var sample = GetSampleFromPackage(packageName, sampleName);
            if (sample == null)
            {
                return false;
            }

            loadedSample = sample.Value;
            ImportAndOpenSample(loadedSample);
            return true;
        }
        
        public void OpenScene(string scenePath)
        {
            EditorSceneManager.OpenScene($"{loadedSample.importPath}/{scenePath}.unity");
        }
        
        private Sample? GetSampleFromPackage(string packageName, string sampleName)
        {
            var samples = Sample.FindByPackage(packageName, null).ToArray();
            if (samples.Length == 0)
            {
                return null;
            }
            
            return samples.First(x => x.displayName == sampleName);
        }

        private void ImportAndOpenSample(Sample quickStartSample)
        {
            if (!quickStartSample.isImported)
            {
                quickStartSample.Import();
                while (!quickStartSample.isImported)
                    Thread.Sleep(1);
            }
        }
    
    }
}
