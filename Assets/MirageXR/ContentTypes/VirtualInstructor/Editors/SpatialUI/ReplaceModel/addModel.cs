using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
    public class addModel : MonoBehaviour
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
            // Todo to implement the model download
            UnityEngine.Debug.Log("URL="+url);
            conformation.SetActive(true);
            close.SetActive(true);
        }
    }
}
