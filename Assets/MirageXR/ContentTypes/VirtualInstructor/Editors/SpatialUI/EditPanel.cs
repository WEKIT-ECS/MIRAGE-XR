using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class EditPanel : MonoBehaviour
    {
        [SerializeField] private Button close;
        [SerializeField] private Button edit;
        [SerializeField] private Button delete;
        void Start()
        {
            close.onClick.AddListener(Close);
            edit.onClick.AddListener(EditObj);
            delete.onClick.AddListener(DeleteObj);
        }

        private void Close()
        {
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
