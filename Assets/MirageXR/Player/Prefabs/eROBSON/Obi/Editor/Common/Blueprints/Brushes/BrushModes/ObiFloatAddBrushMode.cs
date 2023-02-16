namespace Obi
{
    public class ObiFloatAddBrushMode : IObiBrushMode
    {
        ObiBlueprintFloatProperty property;

        public ObiFloatAddBrushMode(ObiBlueprintFloatProperty property)
        {
            this.property = property;
        }

        public string name
        {
            get { return "Add"; }
        }

        public bool needsInputValue
        {
            get { return true; }
        }

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    float currentValue = property.Get(i);
                    float delta = brush.weights[i] * brush.opacity * brush.speed * property.GetDefault();

                    property.Set(i, currentValue + delta * (modified ? -1 : 1));
                }
            }
        }
    }
}