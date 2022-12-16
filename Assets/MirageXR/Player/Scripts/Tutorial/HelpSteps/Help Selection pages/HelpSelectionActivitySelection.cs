using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpSelectionActivitySelection
    {
        private Tutorial _mobileTutorial;
        private HelpSelectionPopup _popup;

        public void Init(HelpSelectionPopup popup, Tutorial mobileTutorial)
        {
            _popup = popup;
            _mobileTutorial = mobileTutorial;

            _popup.CreateNewSelectionButton("How to search for a specific activity").onClick.AddListener(search);
            _popup.CreateNewSelectionButton("How to open an activity").onClick.AddListener(open);
            _popup.CreateNewSelectionButton("How to create a new activity").onClick.AddListener(edit);
            _popup.CreateNewSelectionButton("How to create an account and login").onClick.AddListener(createAccount);
        }


        public void search() {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "activity_search", message = "To search for an activity by name, tap the search menu item below." });
            _mobileTutorial.Show(queue);
        }

        public void open()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { message = "To open an activity find it on the list here and simply tap it!" });
            _mobileTutorial.Show(queue);
        }

        public void edit()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "activity_create", message = "Tap the plus button below to add a new activity" });
            _mobileTutorial.Show(queue);
        }

        public void createAccount()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { id = "user_profile", message = "To login, register or manage your account, tap the profile menu item below." });
            _mobileTutorial.Show(queue);
        }
    }
}
