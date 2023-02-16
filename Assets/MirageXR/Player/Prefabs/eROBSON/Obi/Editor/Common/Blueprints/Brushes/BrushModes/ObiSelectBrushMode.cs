namespace Obi
{
    public class ObiSelectBrushMode : IObiBrushMode
    {
        ObiBlueprintSelected property;
        string customName;

        public ObiSelectBrushMode(ObiBlueprintSelected property, string customName = "Select")
        {
            this.property = property;
            this.customName = customName;
        }

        public string name
        {
            get { return customName; }
        }

        public bool needsInputValue
        {
            get { return true; }
        }

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (brush.weights[i] > 0 && !property.Masked(i))
                    property.Set(i,!modified);
            }
        }
    }
}