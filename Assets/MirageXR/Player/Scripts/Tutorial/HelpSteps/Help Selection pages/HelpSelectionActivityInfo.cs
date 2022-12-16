using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActivityInfo
    {
        private Tutorial _mobileTutorial;
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("What are title/description for").onClick.AddListener(titleAndDescription);
        }

        public void titleAndDescription() 
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "Titles allow you to name your activities, making it clearer for creator and learner what an activity is about. Descriptions allow you to go into even more detail, for example by adding information on the content." });
            _mobileTutorial.Show(queue);
        }
    }
}
