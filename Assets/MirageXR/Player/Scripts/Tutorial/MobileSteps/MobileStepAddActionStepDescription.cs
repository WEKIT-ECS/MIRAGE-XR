using TMPro;

namespace MirageXR
{
    public class MobileStepAddActionStepDescription : MobileStep
    {
        private TMP_InputField descriptionField;

        protected override void Init()
        {
            this.instructionText = "It is also good practice to provide a short description of the Action step for your students, \"Tap\" this text box to start editing the step description.";

            ContentListView clv = RootView.Instance.contentListView;
            this.descriptionField = clv.TxtStepDescription;
            this.highlightedObject = descriptionField.gameObject;

            EventManager.ActionStepDescriptionInputChanged += DescriptionChangedListener;
        }

        private void DescriptionChangedListener()
        {
            if (descriptionField.text != "" && !descriptionField.text.Equals("Add task step description here...​"))
            {
                this.ExitStep();
            }
        }

        protected override void Detach()
        {
            EventManager.ActionStepDescriptionInputChanged -= DescriptionChangedListener;
        }
    }
}
