namespace Obi
{
    public class ObiBlueprintFilterCategory : ObiBlueprintIntProperty
    {
        public ObiActorBlueprintEditor editor;

        public ObiBlueprintFilterCategory(ObiActorBlueprintEditor editor) : base(ObiUtils.MinCategory, ObiUtils.MaxCategory) 
        {
            this.editor = editor;
            brushModes.Add(new ObiIntPaintBrushMode(this));
        }

        public override string name
        {
            get { return "Category"; }
        }

        public override int Get(int index)
        {
            return ObiUtils.GetCategoryFromFilter(editor.blueprint.filters[index]);
        }
        public override void Set(int index, int value)
        {
            editor.blueprint.filters[index] = ObiUtils.MakeFilter(ObiUtils.GetMaskFromFilter(editor.blueprint.filters[index]), value);
        }
        public override bool Masked(int index)
        {
            return !editor.Editable(index);
        }
    }
}
