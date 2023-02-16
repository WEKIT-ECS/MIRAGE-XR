namespace Obi
{
    public class ObiIntPaintBrushMode : IObiBrushMode
    {
        ObiBlueprintIntProperty property;

        public ObiIntPaintBrushMode(ObiBlueprintIntProperty property)
        {
            this.property = property;
        }

        public string name
        {
            get { return "Paint"; }
        }

        public bool needsInputValue
        {
            get { return true; }
        }

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (!property.Masked(i) && brush.weights[i] > (1 - brush.opacity))
                {
                    property.Set(i, property.GetDefault());
                }
            }
        }
    }
}