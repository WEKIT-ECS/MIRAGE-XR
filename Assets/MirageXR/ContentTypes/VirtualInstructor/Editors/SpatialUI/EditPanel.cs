using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class EditPanel : MonoBehaviour
    {
        [SerializeField] private Button close;
        [SerializeField] private Button edit;
        [SerializeField] private Button delete;
        
        // Start is called before the first frame update
        void Start()
        {
            close.onClick.AddListener(Close);
            edit.onClick.AddListener(EditObj);
            delete.onClick.AddListener(DeleteObj);
        }

        private void Close()
        {
            UnityEngine.Debug.Log($"Close {gameObject.name}");
            this.gameObject.SetActive(false);
        }

        private void EditObj()
        {
            UnityEngine.Debug.Log($"Edit {gameObject.name}");
        }

        private void DeleteObj()
        {
            UnityEngine.Debug.Log($"Delete {gameObject.name}");
        }
    }
}
