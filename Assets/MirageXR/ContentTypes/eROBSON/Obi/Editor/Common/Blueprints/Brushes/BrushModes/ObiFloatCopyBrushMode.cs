namespace Obi
{
    public class ObiFloatCopyBrushMode : IObiBrushMode
    {
        ObiBlueprintFloatProperty property;
        public ObiBlueprintFloatProperty source;

        public ObiFloatCopyBrushMode(ObiBlueprintFloatProperty property, ObiBlueprintFloatProperty source)
        {
            this.property = property;
            this.source = source;
        }

        public string name
        {
            get { return "Copy"; }
        }

        public bool needsInputValue
        {
            get { return false; }
        }

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            if (property != null && source != null)
            {
                for (int i = 0; i < brush.weights.Length; ++i)
                {
                    if (!property.Masked(i) && brush.weights[i] > 0)
                    {
                        float currentValue = property.Get(i);
                        float sourceValue = source.Get(i);
                        float delta = brush.weights[i] * brush.opacity * brush.speed * (sourceValue - currentValue);

                        property.Set(i, currentValue + delta * (modified ? -1 : 1));
                    }
                }
            }
        }
    }
}