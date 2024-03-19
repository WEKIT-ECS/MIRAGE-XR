using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

namespace MirageXR.Services.AI_service_backend
{   
    [Serializable]
    public class ConfigurationManager : MonoBehaviour
    {
        private static string _path = "Assets/StreamingAssets/config.json";
        private AIServiceConfiguration _config;
        private AIServices _services;
        
        public IEnumerator LoadEndpointConfiguration(string endpoint, Action<string[]> callback)
        {
            if (_config._endPoints.AsQueryable().Contains(endpoint))
            {
                using var webRequest = UnityWebRequest.Get(_config._servicesURL + endpoint);
                yield return webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                    {
                        string response = webRequest.downloadHandler.text;
                        _services = JsonUtility.FromJson<AIServices>("{\"services\":" + response + "}");
                        foreach (var service in _services.Services)
                        {
                            Debug.Log("Service Name: " + service.Name);
                            if (service.Name == endpoint)
                            {
                                callback?.Invoke(service.Models);
                                yield break;
                            }
                        }
                        break;
                    }
                    case UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError:
                        UnityEngine.Debug.LogError($"Error in the Request to the AI service backend. Endpoint: {endpoint}, Result {webRequest.error}");
                        break;
                }
            }
            else
            {
                throw new Exception($"Endpoint dose not exist! Your input = {endpoint}");
            }
        }
        
        public string GetServicesURL()
        {
            return _config._servicesURL;
        }
        
        public string GetApiKey()
        {
            return _config._apiKey;
        }

        public string[] GetEndpoints()
        {
            return _config._endPoints;
        }
        
        private void Awake()
        {
            try
            {
                string _json = File.ReadAllText(_path);
                _config = JsonUtility.FromJson<AIServiceConfiguration>(_json);
                UnityEngine.Debug.Log("Configuring the ConfigurationManager");
                UnityEngine.Debug.Log(_config._servicesURL);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to load configuration: {ex.Message}");
            }
        }
    }
}
