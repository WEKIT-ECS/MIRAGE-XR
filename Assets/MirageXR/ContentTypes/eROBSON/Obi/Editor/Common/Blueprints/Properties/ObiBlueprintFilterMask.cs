namespace Obi
{
    public class ObiBlueprintFilterMask : ObiBlueprintMaskProperty
    {
        public ObiActorBlueprintEditor editor;

        public ObiBlueprintFilterMask(ObiActorBlueprintEditor editor)
        {
            this.editor = editor;
            brushModes.Add(new ObiIntPaintBrushMode(this));
        }

        public override string name
        {
            get { return "Collides with"; }
        }

        public override int Get(int index)
        {
            return ObiUtils.GetMaskFromFilter(editor.blueprint.filters[index]);
        }
        public override void Set(int index, int value)
        {
            editor.blueprint.filters[index] = ObiUtils.MakeFilter(value,ObiUtils.GetCategoryFromFilter(editor.blueprint.filters[index]));
        }
        public override bool Masked(int index)
        {
            return !editor.Editable(index);
        }
    }
}
