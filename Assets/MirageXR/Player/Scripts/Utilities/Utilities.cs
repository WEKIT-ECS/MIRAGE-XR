using i5.Toolkit.Core.VerboseLogging;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public static class Utilities
    {
        private static readonly Regex RegexFloat = new Regex("[-+]?([0-9]*[.,])?[0-9]+([eE][-+]?\\d+)?");

        /// <summary>
        /// Convert string to Vector3.
        /// </summary>
        /// <param name="input">Vector3 string.</param>
        /// <returns>Vector3 value</returns>
        public static Vector3 ParseStringToVector3(string input)
        {
            var matches = RegexFloat.Matches(input);
            if (matches.Count != 3)
                throw new ArgumentException($"value {(string.IsNullOrEmpty(input) ? "'empty string'" : input)}");

            var temp = Vector3.zero;
            for (var i = 0; i < matches.Count; i++)
            {
                if (float.TryParse(matches[i].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float res))
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
        /// <param name="input">Vector3 string</param>
        /// <param name="value">out value</param>
        /// <returns>returns True if parsing was succeeded</returns>
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
        /// <param name="input">Vector3 string</param>
        /// <returns>returns string  with Vector3 representation in format 0, 0, 0</returns>
        public static string Vector3ToString(Vector3 vector3)
        {
            return $"{vector3.x.ToString(CultureInfo.InvariantCulture)}, {vector3.y.ToString(CultureInfo.InvariantCulture)}, {vector3.z.ToString(CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Convert string to Quaternion.
        /// </summary>
        /// <param name="input">Quaternion string.</param>
        /// <returns>Quaternion value</returns>
        public static Quaternion ParseStringToQuaternion(string input)
        {
            var matches = RegexFloat.Matches(input);
            if (matches.Count != 4)
                throw new ArgumentException($"value: {(string.IsNullOrEmpty(input) ? "'empty string'" : input)}");

            var temp = Quaternion.identity;
            for (var i = 0; i < matches.Count; i++)
                temp[i] = float.Parse(matches[i].Value, NumberStyles.Float, CultureInfo.InvariantCulture);

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
                var parentObject = GameObject.Find(parent); //TODO: possible NRE

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
                AppLog.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// Finds a transform from all the children of a transform.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="id">Child id to look for.</param>
        /// <returns></returns>
        public static Transform FindDeepChild(this Transform parent, string id)
        {
            foreach (Transform child in parent)
            {
                if (child.name == id)
                    return child;

                var result = child.FindDeepChild(id);
                if (result != null)
                    return result;
            }

            return null;
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

        public static Vector3 ConvertEulerAngles(Vector3 angles)
        {
            var output = new Vector3
            {
                x = ConvertSingleAxis(angles.x),
                y = ConvertSingleAxis(angles.y),
                z = ConvertSingleAxis(angles.z)
            };

            return output;
        }

        public static float ConvertSingleAxis(float angle)
        {
            if (angle < 0)
                angle += 360;
            else if (angle > 360)
                angle -= 360;

            return angle;
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

        public static Pose GetPoseInReferenceFrame(Transform measurementObject, Vector3 originPosition, Quaternion originRotation)
        {
            var tempPose = new GameObject("measurementPoint");
            var refFrame = new GameObject("coordinateSystem");

            tempPose.transform.position = measurementObject.position;
            tempPose.transform.rotation = measurementObject.rotation;

            refFrame.transform.position = originPosition;
            refFrame.transform.rotation = originRotation;

            tempPose.transform.SetParent(refFrame.transform);

            var relativePose = new Pose(tempPose.transform.localPosition, tempPose.transform.localRotation);

            UnityEngine.Object.Destroy(tempPose);
            UnityEngine.Object.Destroy(refFrame);

            return relativePose;
        }

        public static bool EulerAnglesAreTheSame(Vector3 eulerOne, Vector3 eulerTwo, float tolerance)
        {
            float difference = Quaternion.Angle(Quaternion.Euler(eulerOne), Quaternion.Euler(eulerTwo));
            bool sameRotation = Mathf.Abs(difference) < tolerance;
            if (!sameRotation) { AppLog.LogDebug("Angles not the same, separated by " + difference + " degrees"); }
            return sameRotation;
        }

        public static Texture2D LoadTexture(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            var fileData = File.ReadAllBytes(filePath);
            var texture2D = new Texture2D(2, 2);
            return texture2D.LoadImage(fileData) ? texture2D : null;
        }

        public static Sprite TextureToSprite(Texture2D texture2d)
        {
            const float pixelsPerUnit = 100.0f;
            var pivot = new Vector2(0.5f, 0.5f);

            return Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), pivot, pixelsPerUnit);
        }


        public static void DeleteAllFilesInDirectory(string directoryName)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryName);

            foreach (FileInfo f in dir.GetFiles())
            {
                f.Delete();
            }

            foreach (DirectoryInfo d in dir.GetDirectories())
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
                AppLog.LogException(e);
            }
        }

        public static async void AsAsyncVoid(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
            }
        }
    }
}
