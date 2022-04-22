using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MirageXR
{
    public class ArlemFetcher : MonoBehaviour
    {

        public Transform ButtonContainer;
        public InputField searchBar;
        public Button Next;
        public Button Prev;

        private string preInupt = "";
        private int pageStart = 0;
        private int pageEnd = 10;
        private int arlemsOnPage = 10;

        private readonly List<JSONNode> _nodes = new List<JSONNode>(); 

        // Use this for initialization
        private void Start()
        {
            Invoke(nameof(DownloadSessionListAndARLEM), 0.5f);

            Next.onClick.AddListener(this.NextOnClick);
            Prev.onClick.AddListener(this.PrevOnClick);
        }

        void OnGUI() {
            if (searchBar.text != preInupt)
            {               
                Invoke(nameof(DownloadSessionListAndARLEM), 0.5f);
            }
            preInupt = searchBar.text;
        }

        public void LoadMainMenu()
        {
            EventManager.PlayerExit();
            SceneManager.LoadScene("AppSelection", LoadSceneMode.Additive);
            CalibrationTool.Instance.Reset();
            SceneManager.UnloadSceneAsync("ArlemLoading");
        }

        public void LoadMainMenuTouch()
        {
            EventManager.Click();
            LoadMainMenu();
        }


        private async void DownloadSessionListAndARLEM()
        {
            
            _nodes.Clear();          
            Debug.Log("Downloading!");

            // Step 1: this retrieves the complete list of sessions from the backend in JSON Format
            JSONNode node = await Network.GetDownloadableSessionsAsync();
            Debug.Log("Downloaded session list");

            Debug.Log(node.Value);

            // Step 2: we iterate through asll downloaded sessions and look for the first one with category=ARLEM
            foreach (JSONNode entry in node.AsArray)
            {
                Debug.Log("entry: " + entry.ToJSON(0));
                if (entry["category"] != null && entry["category"].Value.Equals("ARLEM"))
                {
                    if (entry["key"] != null)
                    {
                        
                        if (searchBar.text == "")
                        {                         // Add node to the list of nodes.
                            _nodes.Add(entry);
                        }
                        else if (entry["description"].ToString().Contains(searchBar.text)) {
                            _nodes.Add(entry);
                        }
                    }
                }
            }
            ResetArlems();
            // If nodes list is not empty...
            if (_nodes.Count > 0)
            {
                Debug.Log(_nodes.Count + " sessions found.");

                if (_nodes.Count > 10)
                {
                    for (int i = pageStart; i < pageEnd; i++)
                    {
                        CreateObject(_nodes[i]);
                    }
                }
                else
                {
                    foreach (var jsonNode in _nodes)
                    {
                        CreateObject(jsonNode);
                    }
                }
           
            }
            else
            {
                Debug.Log("No ARLEM session found.");
            }
           
        }
            
        private void CreateObject(JSONNode jsonNode)
        {
            var downloadPanel = Instantiate(Resources.Load<GameObject>("Prefabs/ActivityLoadPrefab"), Vector3.zero, Quaternion.identity);

            downloadPanel.transform.SetParent(ButtonContainer);
            var rectTransform = downloadPanel.GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localScale = Vector2.one;

            var downloadConfig = downloadPanel.GetComponent<ArlemDownload>();

            downloadConfig.Init("https://wekitproject.appspot.com/storage/serve" + jsonNode["key"].Value, jsonNode["filename"].Value, jsonNode["description"].Value);

            Debug.Log(jsonNode["filename"].Value + " object added to list.");
        }

        private void ResetArlems() {

            foreach (Transform child in ButtonContainer.transform) {
                if (child.transform.GetSiblingIndex() > 2) {
                    
                    Debug.Log("Test: " + child.gameObject.name);
                    GameObject.Destroy(child.gameObject);               
                }
            }
        }
        private void NextOnClick()
        {
            pageStart += arlemsOnPage;
            pageEnd += arlemsOnPage;

            Invoke(nameof(DownloadSessionListAndARLEM), 0.5f);
        }
        private void PrevOnClick()
        {
            if (pageStart > 0) {
                pageStart -= arlemsOnPage;
                pageEnd -= arlemsOnPage;

                Invoke(nameof(DownloadSessionListAndARLEM), 0.5f);
            }            
        }
    }

    
}