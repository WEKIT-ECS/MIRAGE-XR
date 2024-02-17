using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MirageXR
{
    public class HighlightingButton : MonoBehaviour
    {
        [SerializeField] private Button btn;
        private TMP_InputField target;

        public Button Btn => btn;

        public void SetTarget(TMP_InputField target)
        {
            this.target = target;
            Btn.onClick.AddListener(TransferControl);
        }

        private void TransferControl()
        {
            EventManager.NotifyOnHighlightingButtonClicked();
            target.Select();
        }
    }
}
