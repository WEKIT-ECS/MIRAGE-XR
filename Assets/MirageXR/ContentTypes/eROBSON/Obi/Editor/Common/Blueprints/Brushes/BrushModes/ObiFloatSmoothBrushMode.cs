using UnityEngine;

namespace Obi
{
    public class ObiFloatSmoothBrushMode : IObiBrushMode
    {
        ObiBlueprintFloatProperty property;

        public ObiFloatSmoothBrushMode(ObiBlueprintFloatProperty property)
        {
            this.property = property;
        }

        public string name
        {
            get { return "Smooth"; }
        }

        public bool needsInputValue
        {
            get { return false; }
        }

        public void ApplyStamps(ObiBrushBase brush, bool modified)
        {
            var floatProperty = (ObiBlueprintFloatProperty)property;

            float averageValue = 0;
            float totalWeight = 0;

            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    averageValue += floatProperty.Get(i) * brush.weights[i];
                    totalWeight += brush.weights[i];
                }

            }
            averageValue /= totalWeight;

            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    float currentValue = floatProperty.Get(i);
                    float delta = brush.opacity * brush.speed * (Mathf.Lerp(currentValue,averageValue,brush.weights[i]) - currentValue);

                    floatProperty.Set(i, currentValue + delta * (modified ? -1 : 1));
                }
            }

        }
    }
}