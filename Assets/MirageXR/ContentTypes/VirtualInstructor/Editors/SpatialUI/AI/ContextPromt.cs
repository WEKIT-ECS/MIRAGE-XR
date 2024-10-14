using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ContextPromt : MonoBehaviour
    {
        [SerializeField] private Button close;
        [SerializeField] private Button openTooltipBtn; 
        [SerializeField] private Button closeTooltipBtn; 
        
        [SerializeField] private GameObject openTooltip; 
        [SerializeField] private GameObject closeTooltip;
        [SerializeField] private GameObject window;

        void Start()
        {
            close.onClick.AddListener(() => { window.SetActive(false); });
            openTooltipBtn.onClick.AddListener(() =>
            {
                closeTooltip.SetActive(true);
                openTooltip.SetActive(false);

            });
            closeTooltipBtn.onClick.AddListener(() =>
            {
                closeTooltip.SetActive(false);
                openTooltip.SetActive(true);
            });
        }
    }
}
