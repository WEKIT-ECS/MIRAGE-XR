using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace MirageXR.Services.AI_service_backend
{
    public class SpeechInputService
    {
        private AudioClip _myclip;
        private string _targetModel;
        private string _paremters;
        private string _apiKey = new ConfigurationManager.GetApiKey(); //problemo 
        private string _servicesURL = new ConfigurationManager.GetServicesURL(); //problemo 
        private string _endpoint; 
        

        SpeechInputService(AudioClip clip, string targetModel, string endpoint, [CanBeNull] string paremters)
        {
            _myclip = clip;
            _targetModel = targetModel;
            _paremters = paremters;
            _endpoint = endpoint;

        }

        private string getTranscript()
        {
            string json = JsonUtility.ToJson(new { model = _targetModel });
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            var request = new UnityWebRequest(_servicesURL + _endpoint, UnityWebRequest.kHttpVerbPOST);
            request.SetRequestHeader("Authorization", "Token " + _apiKey);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.uploadHandler.contentType = "application/json";

            return "todo";
        }

    }
}



