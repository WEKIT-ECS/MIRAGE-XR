using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ContextPromt : MonoBehaviour
    {

        [SerializeField] private Button closeBtn; 
        [SerializeField] private Button openTooltipBtn; 
        [SerializeField] private Button closeTooltipBtn; 
        
        [SerializeField] private GameObject openTooltip; 
        [SerializeField] private GameObject closeTooltip; 
        
    
        void Start()
        {
         closeBtn.onClick.AddListener(()=> this.gameObject.SetActive(false));
         openTooltipBtn.onClick.AddListener(()=> openTooltip.SetActive(false));
         closeTooltipBtn.onClick.AddListener(()=> closeTooltip.SetActive(false));
        }

      
    }
}
