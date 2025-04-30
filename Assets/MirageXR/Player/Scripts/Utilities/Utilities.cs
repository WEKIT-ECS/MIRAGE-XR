using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MirageXR
{
    public static class Utilities
    {
        private static readonly Regex RegexFloat = new Regex("[-+]?([0-9]*[.,])?[0-9]+([eE][-+]?\\d+)?");

        /// <summary>
        /// Convert string to Vector3.
        /// </summary>
        /// <param name="input">Vector3 string.</param>
        /// <returns>Vector3 value.</returns>
        public static Vector3 ParseStringToVector3(string input)
        {
            var matches = RegexFloat.Matches(input);
            if (matches.Count != 3)
            {
                throw new ArgumentException($"value {(string.IsNullOrEmpty(input) ? "'empty string'" : input)}");
            }

            var temp = Vector3.zero;
            for (var i = 0; i < matches.Count; i++)
            {
                if (float.TryParse(matches[i].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var res))
                {
                    temp[i] = res;
                }
                else
                {
                    temp[i] = float.Parse(matches[i].Value);
                }
            }

            return temp;
        }

        /// <summary>
        /// Convert string to Vector3.
        /// </summary>
        /// <param name="input">Vector3 string.</param>
        /// <param name="value">out value.</param>
        /// <returns>returns True if parsing was succeeded.</returns>
        public static bool TryParseStringToVector3(string input, out Vector3 value)
        {
            try
            {
                value = ParseStringToVector3(input);
                return true;
            }
            catch (Exception)
            {
                value = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Convert Vector3 to string.
        /// </summary>
        /// <param name="input">Vector3 string.</param>
        /// <returns>returns string  with Vector3 representation in format 0, 0, 0.</returns>
        public static string Vector3ToString(Vector3 vector3)
        {
            return $"{vector3.x.ToString(CultureInfo.InvariantCulture)}, {vector3.y.ToString(CultureInfo.InvariantCulture)}, {vector3.z.ToString(CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Convert string to Quaternion.
        /// </summary>
        /// <param name="input">Quaternion string.</param>
        /// <returns>Quaternion value.</returns>
        public static Quaternion ParseStringToQuaternion(string input)
        {
            var matches = RegexFloat.Matches(input);
            if (matches.Count != 4)
            {
                throw new ArgumentException($"value: {(string.IsNullOrEmpty(input) ? "'empty string'" : input)}");
            }

            var temp = Quaternion.identity;
            for (var i = 0; i < matches.Count; i++)
            {
                temp[i] = float.Parse(matches[i].Value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            return temp;
        }

        /// <summary>
        /// Convert string to Quaternion.
        /// </summary>
        /// <param name="input">Quaternion string</param>
        /// <param name="value">out value</param>
        /// <returns>returns True if parsing was succeeded</returns>
        public static bool TryParseStringToQuaternion(string input, out Quaternion value)
        {
            try
            {
                value = ParseStringToQuaternion(input);
                return true;
            }
            catch (Exception)
            {
                value = Quaternion.identity;
                return false;
            }
        }

        /// <summary>
        /// Create a new object and place it under the parent.
        /// </summary>
        /// <param name="id">Id for the created game object.</param>
        /// <param name="parent">Parent transform.</param>
        public static GameObject CreateObject(string id, Transform parent)
        {
            // Create a new game object and name it after the id.
            var temp = new GameObject(id);

            // Place it under the parent.
            temp.transform.SetParent(parent);

            // Set zero tranform.
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localEulerAngles = Vector3.zero;
            temp.transform.localScale = Vector3.one;

            // Return created game object.
            return temp;
        }

        /// <summary>
        /// Create a new object and place it under the parent.
        /// </summary>
        /// <param name="id">Id for the created game object.</param>
        /// <param name="parent">Parent id.</param>
        public static GameObject CreateObject(string id, string parent)
        {
            try
            {
                var parentObject = GameObject.Find(parent);

                if (parentObject == null)
                {
                    throw new ArgumentException($"Object {parent} not found.");
                }

                // Create a new game object and name it after the id.
                var temp = new GameObject(id);

                // Place it under the parent.
                temp.transform.SetParent(parentObject.transform);

                // Set zero tranform.
                temp.transform.localPosition = Vector3.zero;
                temp.transform.localEulerAngles = Vector3.zero;
                temp.transform.localScale = Vector3.one;

                // Return created game object.
                return temp;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// Finds a transform from all the children of a transform.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="id">Child tag to look for.</param>
        /// <returns></returns>
        public static Transform FindDeepChildTag(this Transform parent, string id)
        {
            foreach (Transform child in parent)
            {
                if (child.CompareTag(id))
                    return child;

                var result = child.FindDeepChildTag(id);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Vector3 CalculateOffset(Vector3 anchorPosition, Quaternion anchorRotation, Vector3 originPosition, Quaternion originRotation) //TODO: Looks like it can be replaced by Transform.InverseTransformPoint(...)
        {
            // Some black magic for getting the offset.
            var anchorDummy = new GameObject("AnchorDummy");
            var targetDummy = new GameObject("TargetDummy");

            anchorDummy.transform.position = anchorPosition;
            anchorDummy.transform.rotation = anchorRotation;
            targetDummy.transform.position = originPosition;
            targetDummy.transform.rotation = originRotation;

            anchorDummy.transform.SetParent(targetDummy.transform);

            Vector3 offset = anchorDummy.transform.localPosition;

            GameObject.Destroy(anchorDummy);
            GameObject.Destroy(targetDummy);

            return offset;
        }

        public static bool EulerAnglesAreTheSame(Vector3 eulerOne, Vector3 eulerTwo, float tolerance)
        {
            var difference = Quaternion.Angle(Quaternion.Euler(eulerOne), Quaternion.Euler(eulerTwo));
            var sameRotation = Mathf.Abs(difference) < tolerance;
            if (!sameRotation)
            {
                Debug.LogDebug("Angles not the same, separated by " + difference + " degrees");
            }

            return sameRotation;
        }

        public static Texture2D LoadTexture(string filePath, bool readable = true)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var fileData = File.ReadAllBytes(filePath);
            var texture2D = new Texture2D(2, 2);
            return texture2D.LoadImage(fileData, !readable) ? texture2D : null;
        }

        public static Sprite TextureToSprite(Texture2D texture2d)
        {
            const float pixelsPerUnit = 100.0f;
            var pivot = new Vector2(0.5f, 0.5f);

            return Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), pivot, pixelsPerUnit);
        }

        public static void DeleteAllFilesInDirectory(string directoryName)
        {
            var dir = new DirectoryInfo(directoryName);

            foreach (var f in dir.GetFiles())
            {
                f.Delete();
            }

            foreach (var d in dir.GetDirectories())
            {
                DeleteAllFilesInDirectory(d.FullName);
                d.Delete();
            }
        }

        public static void CopyEntireFolder(string folderPath, string destinationPath)
        {
            try
            {
                foreach (var dirPath in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(folderPath, destinationPath));
                }

                foreach (var newPath in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(folderPath, destinationPath), true);
                }
            }
            catch (IOException e)
            {
                Debug.LogException(e);
            }
        }

        public static async void AsAsyncVoid(this Task task)
        {
            try
            {
                await task;
            }
            catch (TaskCanceledException e)
            {
                Debug.Log(e.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static T FindOrCreateComponent<T>(GameObject holder = null) where T : Component
        {
            var component = Object.FindFirstObjectByType<T>();
            if (!component)
            {
                if (!holder)
                {
                    holder = new GameObject(typeof(T).FullName);
                }

                component = holder.GetComponent<T>();
                if (!component)
                {
                    component = holder.AddComponent<T>();
                }
            }

            return component;
        }

        public static Pose GetPose(this GameObject gameObject)
        {
            return gameObject.transform.GetPose();
        }

        public static Pose GetPose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }

        public static Pose GetLocalPose(this GameObject gameObject)
        {
            return gameObject.transform.GetLocalPose();
        }

        public static Pose GetLocalPose(this Transform transform)
        {
            return new Pose(transform.localPosition, transform.localRotation);
        }

        public static void SetPose(this GameObject gameObject, Pose pose)
        {
            gameObject.transform.SetPose(pose);
        }

        public static void SetPose(this Transform transform, Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        public static void SetLocalPose(this GameObject gameObject, Pose pose)
        {
            gameObject.transform.SetLocalPose(pose);
        }

        public static void SetLocalPose(this Transform transform, Pose pose)
        {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
        }

        public static string GetResourceName(string path)
        {
            const string resourcesFolder = "Resources/";
            var split = path.Split(resourcesFolder);
            if (split.Length > 1)
            {
                path = split[^1];
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            var dir = Path.GetDirectoryName(path);
            return Path.Combine(dir, fileName);
        }

        public static Vector3 ToClosestToStepVector3(Vector3 vector3, float step)
        {
            var result = Vector3.zero;
            result.x = ToClosestToStepValue(vector3.x, step);
            result.y = ToClosestToStepValue(vector3.y, step);
            result.z = ToClosestToStepValue(vector3.z, step);
            return result;
        }

        public static float ToClosestToStepValue(float value, float step)
        {
            var entire = (int)(value / step);
            var residue = value % step;
            if (residue > step * 0.5f)
            {
                entire++;
            }

            return step * entire;
        }
    }
}
