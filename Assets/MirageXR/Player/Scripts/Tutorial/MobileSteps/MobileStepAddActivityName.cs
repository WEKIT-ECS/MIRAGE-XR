using TMPro;

namespace MirageXR
{
    public class MobileStepAddActivityName : MobileStep
    {
        private TMP_InputField nameField;

        protected override void Init()
        {
            this.instructionText = "You should always give your learning activity a proper name to be able to identify it later. You can \"Tap\" this field to rename your learning activity.";
            nameField = RootView.Instance.stepsListView.ActivityNameField;
            this.highlightedObject = nameField.gameObject;

            EventManager.ActivityRenamed += OnActivityRename;
        }

        private void OnActivityRename()
        {
            if (nameField.text != "")
            {
                this.ExitStep();
            }
        }

        protected override void Detach()
        {
            EventManager.ActivityRenamed -= OnActivityRename;
        }
    }
}
