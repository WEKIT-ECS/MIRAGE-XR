using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
    public class AddModelPanel : MonoBehaviour
    {
        
        [Header("GameObject")]
        [SerializeField] private GameObject close;
        [SerializeField] private GameObject customLinkOpen;
        [SerializeField] private GameObject customLinkClose;
        [SerializeField] private GameObject conformation;
        [SerializeField] private GameObject addModelGO;

        [Header("Buttons")]
        [SerializeField] private Button addModelBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button closeWindowBtn;
        [SerializeField] private Button openCustomLink;
        [SerializeField] private Button closeCustomLink;

        [FormerlySerializedAs("_inputField")] [Header("Input Field")] [SerializeField]
        private TMP_InputField inputField;

        public delegate void ModelSelectedHandler(string modelUrl);
        public event ModelSelectedHandler ModelSelected;
        
        void Start()
        {
            closeWindowBtn.onClick.AddListener(()=> this.gameObject.SetActive(false));
            addModelBtn.onClick.AddListener(()=>AddmodelToLibrary(inputField.text));
            openCustomLink.onClick.AddListener(() => { customLinkOpen.SetActive(true); customLinkClose.SetActive(false); });
            closeCustomLink.onClick.AddListener(() => { customLinkClose.SetActive(true); customLinkOpen.SetActive(false);});
            closeBtn.onClick.AddListener(()=> this.gameObject.SetActive(false));
            inputField.onSubmit.AddListener((value) => addModelGO.SetActive(true)); 
        }
   
        private void AddmodelToLibrary(string url)
        {
			if (string.IsNullOrWhiteSpace(url))
			{
                return;
			}
			UnityEngine.Debug.Log("URL="+url);
            RootObject.Instance.AvatarLibraryManager.AddAvatar(url);
            ModelSelected?.Invoke(url);
            conformation.SetActive(true);
            close.SetActive(true);
        }
    }
}
