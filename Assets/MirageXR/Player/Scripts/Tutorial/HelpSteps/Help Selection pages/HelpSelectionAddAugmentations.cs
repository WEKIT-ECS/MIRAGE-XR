using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionAddAugmentations
    {
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup)
        {
            _popup = popup;
            _popup.CreateNewSelectionButton("For more info on each augmentation type, open their window by selecting them on the list and then click their info").onClick.AddListener(Exit);
        }

        private void Exit()
        {
            _popup.Close();
        }
    }
}
