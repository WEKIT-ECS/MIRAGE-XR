using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml.Serialization;
using System.IO;

namespace MirageXR
{

    public class GhostHandPlayer : MirageXRPrefab
    {


        [Tooltip ("Filename of the xml file containing the GhostHand recording")]
        public string filename = "ghosthand_recording";
        [Tooltip ("If set, the content is read from the application's 'LocalState' folder on the HoloLens, otherwise from the project's 'Resources' folder")]
        public bool useExternalSource = false;
        [Tooltip ("3D model of right hand")]
        public GameObject rightHand;
        [Tooltip ("3D model of left hand")]
        public GameObject leftHand;
        [Tooltip ("3D model of a gaze-tracked head. Set to null to not show the head.")]
        public GameObject head = null;
        [Tooltip ("How fast is the ghost hand recording replayed. Default is 25 fps")]
        public int fps = 25;
        [Tooltip ("Should the player keep repeating the recording?")]
        public bool looping = false;

        //Private variables
        bool _isPlaying = false;
        GameObject originPose;
        float _startTime = 0;
        int _totalFrames = 0;
        int _currentFrame = 0;

        List<HandsDataFrame> _recordedData;
        int _startPoint;
        int _endPoint;
        float _frameTime = 0.04f;

        public override bool Init (ToggleObject obj)
        {
            originPose = new GameObject();
            SetOriginFromArlem(obj);


            // Check that url is not empty.
            if (string.IsNullOrEmpty (obj.url))
            {
                Debug.Log ("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent (obj))
            {
                Debug.Log ("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            // Set scale.
            transform.localScale = Vector3.one;

            // Load hands from resources.
            if (obj.url.StartsWith("resources://"))
            {
                obj.url.Replace("resources://", "");
            }

            // Set looping.
            looping = false;

            // Load ghost hand recording.
            if (LoadRecording(obj.url, true))
            {
                // Start playing starting using the poi as the origin.
                StartPlaying(_recordedData, originPose.transform);
            }
            else
            {
                Debug.Log("Error loading ghost recording");
            }

            // Set frame time.
            _frameTime = 1f / (float) fps;

            // If all went well, return true.
            return true;
        }

        void Update ()
        {
            if (_isPlaying)
            {
                // Select the correct frame to display according to the elapsed play time and speed
                _currentFrame = (int) ((Time.time - _startTime) / _frameTime);
                if (_currentFrame < _totalFrames)
                {
                    //Debug.Log( "frame: " + _currentFrame );
                    PlayFrame (_recordedData [_currentFrame]);
                }
                else
                {
                    /*
                    if (looping == true)
                    {
                        StartPlaying (_myCoords);
                    }
                    else
                    {
                        Stop ();
                    }
                    */
                }
            }
        }

        /// <summary>
        /// This function sets the location for the ghost hand recording and starts playing it
        /// </summary>
        /// <param name="PMC">The origin location where the recording will be played.</param>
        public void StartPlaying (List<HandsDataFrame> myoData, Transform PMC)
        {
            _currentFrame = 0;
            _startTime = Time.time;
            _isPlaying = true;
        }

        /// <summary>
        /// This function stops playing the recording; or if it was already stopped
        /// will start playing from the frame it was stopped on.
        /// </summary>
        public void Stop ()
        {
            if (_isPlaying)
            {
                _isPlaying = false;
            }
            else
            {
                _startTime = Time.time - _currentFrame * 0.04f;
                _isPlaying = true;
            }
        }

        /// <summary>
        /// This function sets the position of the ghost to match the recorded data
        /// </summary>
        /// <param name="tempData">Recorded data for the current frame.</param>
        void PlayFrame(HandsDataFrame tempData)
        {

            if (rightHand != null)
            {
                rightHand.transform.position = originPose.transform.TransformPoint (tempData.RightHandPos);
                rightHand.transform.rotation = tempData.RightHandRot;
            }
            if (leftHand != null)
            {
                leftHand.transform.position = originPose.transform.TransformPoint (tempData.LeftHandPos);
                leftHand.transform.rotation = tempData.LeftHandRot;
            }

        }


        private void SetOriginFromArlem(ToggleObject obj)
        {
            Debug.Log("pos" + obj.position);
            Debug.Log("rot" + obj.rotation);

            string posTrim = obj.position.Substring(1, obj.position.Length - 2);
            string[] pn = posTrim.Split(',');
            originPose.transform.position = new Vector3(float.Parse(pn[0]), float.Parse(pn[1]), float.Parse(pn[2]));

            string rotTrim = obj.rotation.Substring(1, obj.rotation.Length - 2);
            string[] rt = rotTrim.Split(',');
            originPose.transform.rotation = new Quaternion(float.Parse(rt[0]), float.Parse(rt[1]), float.Parse(rt[2]), float.Parse(rt[3]));
            originPose.transform.localScale = new Vector3(obj.scale, obj.scale, obj.scale);
        }


        /// <summary>
        /// Loads previously saved recorded data from a file into the temporary data
        /// from a specified file location.
        /// </summary>
        /// <param name="filename">Name of the to be loaded file.</param>
        /// <param name="useExternalRecordingSource">If true, load hand recording from application's LocalState folder, if false, load from project resources.</param>
        /// <returns>>Returns true, if loading a recording was successful.</returns>
        public bool LoadRecording (string filename, bool useExternalRecordingSource = false)
        {
            useExternalSource = useExternalRecordingSource;

            if (useExternalSource == true)
            {
                string completeFilename = "file://" + Application.persistentDataPath + "/" + filename;
                if (File.Exists (completeFilename))
                {
                    FileStream file = File.Open (completeFilename, FileMode.Open);
                    TextReader tR = new StreamReader (file);
                    XmlSerializer xS = new XmlSerializer (typeof (List<HandsDataFrame>));
                    _recordedData = (List<HandsDataFrame>) xS.Deserialize (tR);
                    if (_recordedData != null)
                    {
                        _totalFrames = _recordedData.Count;
                        if (_totalFrames > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (filename.EndsWith (".xml"))
                {
                    filename = filename.Substring (0, filename.Length - 4);
                }
                TextAsset asset = Resources.Load (filename) as TextAsset;
                if (asset != null)
                {
                    Debug.Log ("Read GhostHand recording data from file:" + filename);
                    Stream stream = new MemoryStream (asset.bytes);
                    TextReader tR = new StreamReader (stream);
                    XmlSerializer xS = new XmlSerializer (typeof (List<HandsDataFrame>));
                    _recordedData = (List<HandsDataFrame>) xS.Deserialize (tR);
                    if (_recordedData != null)
                    {
                        _totalFrames = _recordedData.Count;
                        if (_totalFrames > 0)
                        {
                            print (_recordedData [0]);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Debug.Log ("Couldn't load file: " + filename + "!");
                    return false;
                }
            }
        }
    }
}