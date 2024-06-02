using System.Collections.Generic;

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

            _popup.CreateNewSelectionButton("   ").onClick.AddListener(Search);
            _popup.CreateNewSelectionButton("How tosfy").onClick.AddListener(Open);
            _popup.CreateNewSelectionButton("How to create a new activity").onClick.AddListener(Edit);
            //TODO: Remove the one below, only for testing
            _popup.CreateNewSelectionButton("How to view a new activity").onClick.AddListener(View);
            _popup.CreateNewSelectionButton("How to create an account and login").onClick.AddListener(CreateAccount);
        }

        private void Search()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "activity_search", Message = "To search for an activity by name, tap the search menu item below." });
            _mobileTutorial.Show(queue);
        }

        private void Open()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Message = "To open an activity find it on the list here and simply tap it!" });
            _mobileTutorial.Show(queue);
        }

        private void Edit()
        {
            _popup.Close();
            //TutorialManager.Instance.StartNewMobileEditingTutorial();
            HelpSelectionMaster master = new HelpSelectionMaster();
            master.TestWrite();
        }
        private void View()
        {
            _popup.Close();
            HelpSelectionMaster master = new HelpSelectionMaster();
            master.TestRead();
        }

        private void CreateAccount()
        {
            _popup.Close();
            var queue = new Queue<TutorialModel>();
            queue.Enqueue(new TutorialModel { Id = "", Message = "To login, register or manage your account, tap the profile menu item below." });
            _mobileTutorial.Show(queue);
        }
    }
}
