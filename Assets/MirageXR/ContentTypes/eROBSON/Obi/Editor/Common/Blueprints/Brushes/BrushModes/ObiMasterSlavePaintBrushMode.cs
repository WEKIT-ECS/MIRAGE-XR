namespace Obi
{
    public class ObiMasterSlavePaintBrushMode : IObiBrushMode
    {
        ObiBlueprintIntProperty property;

        public ObiMasterSlavePaintBrushMode(ObiBlueprintIntProperty property)
        {
            this.property = property;
        }

        public string name
        {
            get { return "Master/Slave paint"; }
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
                    int currentValue = property.Get(i);

                    if (modified)
                        currentValue &= ~(int)(1 << property.GetDefault());
                    else currentValue |= (int)(1 << property.GetDefault());

                    property.Set(i, currentValue);
                }
            }
        }
    }
}