using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SidebarView : MonoBehaviour
    {
        [SerializeField] private Button _buttonSidebarCollapse;
        [SerializeField] private Button _buttonSidebarExpand;
        [SerializeField] private GameObject _sidebarOpened;
        [SerializeField] private GameObject _sidebarClosed;
        
        private void Awake()
        {
            _sidebarOpened.SetActive(true);
            _sidebarClosed.SetActive(false);
            _buttonSidebarCollapse.onClick.AddListener(OnSidebarCollapse); 
            _buttonSidebarExpand.onClick.AddListener(OnSidebarExpand);
        }

        private void OnSidebarExpand()
        {
            _sidebarOpened.SetActive(true);
            _sidebarClosed.SetActive(false);
        }

        private void OnSidebarCollapse()
        {
            _sidebarOpened.SetActive(false);
            _sidebarClosed.SetActive(true);
        }
    }
}
