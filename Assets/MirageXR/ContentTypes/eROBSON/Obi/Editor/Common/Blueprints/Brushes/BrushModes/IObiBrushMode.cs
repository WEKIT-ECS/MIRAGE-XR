namespace Obi
{
    public interface IObiBrushMode
    {
        string name{get;}
        bool needsInputValue{ get; }
        void ApplyStamps(ObiBrushBase brush, bool modified);
    }
}
