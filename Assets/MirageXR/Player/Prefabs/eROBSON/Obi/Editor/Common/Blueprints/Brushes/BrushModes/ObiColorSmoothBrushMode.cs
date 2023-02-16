using UnityEngine;

namespace Obi
{
    public class ObiColorSmoothBrushMode : IObiBrushMode
    {
        ObiBlueprintColorProperty property;

        public ObiColorSmoothBrushMode(ObiBlueprintColorProperty property)
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
            Color averageValue = Color.black;
            float totalWeight = 0;

            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    averageValue += property.Get(i) * brush.weights[i];
                    totalWeight += brush.weights[i];
                }

            }
            averageValue /= totalWeight;

            for (int i = 0; i < brush.weights.Length; ++i)
            {
                if (!property.Masked(i) && brush.weights[i] > 0)
                {
                    Color currentValue = property.Get(i);
                    Color delta = brush.opacity * brush.speed * (Color.Lerp(currentValue, averageValue, brush.weights[i]) - currentValue);

                    property.Set(i, currentValue + delta * (modified ? -1 : 1));
                }
            }

        }
    }
}