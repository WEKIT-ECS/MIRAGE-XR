using TMPro;

namespace MirageXR
{
    public class MobileStepAddActionStepTitle : MobileStep
    {
        private TMP_InputField titleField;

        protected override void Init()
        {
            this.instructionText = "After creating a new Action, it is always good practice to provide a title for the Action step for your students, \"Tap\" this text box to give the step a title.";

            ContentListView clv = RootView.Instance.contentListView;
            this.titleField = clv.TxtStepName;
            this.highlightedObject = titleField.gameObject;

            EventManager.ActionStepTitleChanged += TitleChangedListener;
        }

        private void TitleChangedListener()
        {
            if (titleField.text != "" && !titleField.text.Equals("Action Step 1"))
            {
                this.ExitStep();
            }
        }

        protected override void Detach()
        {
            EventManager.ActionStepTitleChanged -= TitleChangedListener;
        }
    }
}
