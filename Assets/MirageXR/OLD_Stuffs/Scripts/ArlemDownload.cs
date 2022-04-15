using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ArlemDownload : MonoBehaviour
    {
        private string _targetDir;

        private string _downloadUrl;
        private string _filename;
        private string _description;

        private string _id;
        private string _folder;

        public GameObject LoadButton;
        public GameObject CheckMark;

        private void Start()
        {
            _targetDir = Application.persistentDataPath;

            LoadButton.SetActive(true);
            CheckMark.SetActive(false);
        }

        public void Init(string url, string filename, string description)
        {
            _targetDir = Application.persistentDataPath;

            _downloadUrl = url;
            _filename = filename;
            _description = description;

            _id = _filename.Substring(0, _filename.Length - 4); // Remove the .zip to get the session id.
            Debug.Log("filename: " + _filename + ", _id:" + _id + ", _targetDir:" + _targetDir);

            _folder = Path.Combine(_targetDir, _id);


            gameObject.name = filename;
            transform.FindDeepChild("ActivityTitle").GetComponent<Text>().text = description;

            // Indicate if session already stored locally.
            CheckMark.SetActive(Directory.Exists(_folder));
        }

        public async void DownloadArlem()
        {
            EventManager.Click();

            var activity = Path.Combine(_targetDir, _id + "-activity.json");
            var workplace = Path.Combine(_targetDir, _id + "-workplace.json");

            Debug.Log("\n\n\n\nDOWNLOAD FOLDER PATH: " + _folder + "\n\n\n\n");

            // If session already stored...
            if (Directory.Exists(_folder))
            {
                Debug.Log("Session already stored, deleting...");
                Directory.Delete(_folder, true); // Remove session folder.

                // Destroy ARLEM files.
                if(File.Exists(activity))
                    File.Delete(activity);

                if (File.Exists(workplace))
                    File.Delete(workplace);

                Debug.Log("Duplicates deleted.");
            }


            bool success = await Network.DownloadAndDecompressPlayerAsync(_downloadUrl, _filename, _targetDir);
            if (success)
            {
                Debug.Log("downloaded " + _filename + "to " + _targetDir + " from " + _downloadUrl);
                LoadButton.SetActive(false);
                CheckMark.SetActive(true);
            }
            else
            {
                Debug.Log("DOWNLOAD FAILED " + _filename + "to " + _targetDir + " from " + _downloadUrl);
            }
        }
    }
}

