using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActionInfo
    {
        private Tutorial _mobileTutorial;
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("What are titles/descriptions for").onClick.AddListener(Explain);
        }

        private void Explain()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Message = "You can make the same augmentation to be present in multiple steps. Tap on the settings of the augmentation, choose keep-alive, and select the range of steps." });
            _mobileTutorial.Show(queue);
        }
    }
}
