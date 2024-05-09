using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class Speechsettings : MonoBehaviour
    {
        [SerializeField] private GameObject aiPromt;
        [SerializeField] private GameObject vcoice;
        [SerializeField] private GameObject model;
        [SerializeField] private GameObject language;
        private string aiPromtData = "Enter text";
        private ObjectData vcoiceData  = new ObjectData();
        private ObjectData modelData  = new ObjectData();
        private ObjectData languageData  = new ObjectData();

        public void Start()
        {
            // todo Call to the Data model and set the data objeckts
            
            
        }

        public void UpdateAIPromt(string newText)
        {
            
        }
        
        public void UpdateVcoice(ObjectData obj)
        {
            
        }
        
        public void UpdateModel(ObjectData obj)
        {
            
        }
        
        public void UpdateLanguage(ObjectData obj)
        {
            
        }

        
        
       
        public String GetAiPromt()
        {
            return aiPromtData;
        }
        public ObjectData GetVocie()
        {
            return modelData
        }
        public ObjectData GetModel()
        {
            return vcoiceData;
        }
        public ObjectData GetLanguage()
        {
            return languageData; 
        }
    }
}
