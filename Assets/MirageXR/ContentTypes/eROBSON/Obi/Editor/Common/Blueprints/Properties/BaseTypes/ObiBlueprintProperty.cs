using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Obi
{
    public abstract class ObiBlueprintPropertyBase
    {
        protected List<IObiBrushMode> brushModes = new List<IObiBrushMode>();
        private int selectedBrushMode;

        public abstract string name
        {
            get;
        }

        public abstract void PropertyField();
        public virtual void VisualizationOptions(){}
        public virtual void OnSceneRepaint(){}

        public abstract bool Equals(int firstIndex, int secondIndex);

        public abstract void GetDefaultFromIndex(int index);
        public abstract void SetDefaultToIndex(int index);
        public virtual bool Masked(int index)
        {
            return false;
        }

        public virtual void RecalculateMinMax() { }
        public virtual Color ToColor(int index) { return Color.white; }

        protected void Initialize(ObiBrushBase paintBrush)
        {
            // Initialize the brush if there's no brush mode set:
            if (paintBrush.brushMode == null && brushModes.Count > 0)
            {
                selectedBrushMode = 0;
                paintBrush.brushMode = brushModes[selectedBrushMode];
            }
        }

        public void OnSelect(ObiBrushBase paintBrush)
        {
            // Upon selecting the property, change to the last selected brush mode:
            if (brushModes.Count > selectedBrushMode)
                paintBrush.brushMode = brushModes[selectedBrushMode];

        }

        public void BrushModes(ObiBrushBase paintBrush)
        {
            Initialize(paintBrush);

            GUIContent[] contents = new GUIContent[brushModes.Count];
            for (int i = 0; i < brushModes.Count; ++i)
                contents[i] = new GUIContent(brushModes[i].name);

            EditorGUI.BeginChangeCheck();
            selectedBrushMode = ObiEditorUtils.DoToolBar(selectedBrushMode, contents);
            if (EditorGUI.EndChangeCheck())
            {
                paintBrush.brushMode = brushModes[selectedBrushMode];
            }
        }
    }

    public abstract class ObiBlueprintProperty<T> : ObiBlueprintPropertyBase
    {
        protected T value;

        public T GetDefault() { return value; }
        public override void GetDefaultFromIndex(int index) { value = Get(index); }
        public override void SetDefaultToIndex(int index) { Set(index, value); }

        public abstract T Get(int index);
        public abstract void Set(int index, T value);
    }
}
