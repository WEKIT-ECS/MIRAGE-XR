using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class AIPanel : MonoBehaviour
    {
        [SerializeField] private Button close;

        void Start()
        {
          close.onClick.AddListener(() => { gameObject.SetActive(false); });
        }
    }
}
