using System.Collections;
using System.IO;
using System.Linq;
using MirageXR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests
{
    public class ActivityParserTests
    {

        //[UnityTest]
        //public IEnumerator JsonFile_Exist_Test()
        //{
        //    // Get all the files inside the application path and sort them by creation date.
        //    var FileExist = false;
        //    var sortedFileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles().ToList();
        //    foreach (var file in sortedFileInfos)
        //    {
        //        // Only interested in the activity json files...
        //        if (file.Name.ToLower().EndsWith("_activity.json") ||
        //            file.Name.ToLower().EndsWith("-activity.json") ||
        //            file.Name.ToLower().EndsWith("activity.json"))
        //        {
        //            FileExist = true;
        //        }

        //    }

        //    Assert.IsTrue(FileExist);
        //    yield return null;
        //}


        [UnityTest]
        public IEnumerator JsonFile_NoneEmptyValues_Test()
        {
            // Get all the files inside the application path and sort them by creation date.
            var sortedFileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles().ToList();
            foreach (var file in sortedFileInfos)
            {
                // Only interested in the activity json files...
                if (file.Name.ToLower().EndsWith("_activity.json") ||
                    file.Name.ToLower().EndsWith("-activity.json") ||
                    file.Name.ToLower().EndsWith("activity.json"))
                {
                    // Read in the presumed activity json.
                    var url = File.ReadAllText(Path.Combine(Application.persistentDataPath, file.Name));

                    // Convert the file into an activity object.
                    var activity = JsonUtility.FromJson<Activity>(url);

                    Assert.IsNotEmpty(activity.id);
                    Assert.IsNotEmpty(activity.name);
                    //Assert.IsNotEmpty(activity.language);
                    Assert.IsNotEmpty(activity.start);
                    Assert.IsNotEmpty(activity.workplace);
                }
            }

            yield return null;
        }


        [UnityTest]
        public IEnumerator JsonFile_SessionObjectInTheScene_Test()
        {
            // Get all the files inside the application path and sort them by creation date.
            var sortedFileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles().ToList();
            foreach (var file in sortedFileInfos)
            {
                // Only interested in the activity json files...
                if (file.Name.ToLower().EndsWith("_activity.json") ||
                    file.Name.ToLower().EndsWith("-activity.json") ||
                    file.Name.ToLower().EndsWith("activity.json"))
                {
                    UnitTestManager unitTestManager = new GameObject().AddComponent<UnitTestManager>();

                    Assert.IsNotNull(unitTestManager.ObjectExists(file.Name));
                }
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator JsonFile_workplaceFileExist_Test()
        {
            // Get all the files inside the application path and sort them by creation date.
            var sortedFileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles().ToList();
            foreach (var file in sortedFileInfos)
            {
                // Only interested in the activity json files...
                if (file.Name.ToLower().EndsWith("_activity.json") ||
                    file.Name.ToLower().EndsWith("-activity.json") ||
                    file.Name.ToLower().EndsWith("activity.json"))
                {

                    // Read in the presumed activity json.
                    var url = File.ReadAllText(Path.Combine(Application.persistentDataPath, file.Name));
                    // Convert the file into an activity object.
                    var activity = JsonUtility.FromJson<Activity>(url);

                    var workplaceExist = false;
                    var path = Path.Combine(Application.persistentDataPath,  activity.id);
                    if (File.Exists($"{path}_workplace.json") ||
                        File.Exists($"{path}-workplace.json") ||
                        File.Exists($"{path}workplace.json"))
                    {
                        workplaceExist = true;
                    }

                    Assert.IsTrue(workplaceExist);

                }
            }

            yield return null;
        }


        [TearDown]
        public void AfterTest()
        {
            foreach (var gameObject in GameObject.FindObjectsOfType<UnitTestManager>())
                //destroy all file which are created for testing
                Object.DestroyImmediate(gameObject);
        }

    }
}
