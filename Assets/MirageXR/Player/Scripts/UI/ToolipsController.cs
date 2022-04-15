using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MirageXR
{
    public class ToolipsController : MonoBehaviour
    {
        [SerializeField] private Text tipsText;

        public void SetTipText(string tooltip)
        {
            tipsText.text = $"Say \"Hi Mirage, {tooltip}\"";
        }

    }
}
