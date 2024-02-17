using UnityEngine;
using UnityEditor;

namespace Obi
{
    public abstract class ObiBlueprintMaskProperty : ObiBlueprintIntProperty
    {
        public ObiBlueprintMaskProperty() : base(null,null)
        {
        }

        public override void PropertyField()
        {
            value = EditorGUILayout.MaskField(name, value, ObiUtils.categoryNames);
        }

        private int MathMod(int a, int b)
        {
            return (Mathf.Abs(a * b) + a) % b;
        }

        public override Color ToColor(int index)
        {
            int colorIndex = MathMod(Get(index),ObiUtils.colorAlphabet.Length);
            return ObiUtils.colorAlphabet[colorIndex];
        }
    }
}
