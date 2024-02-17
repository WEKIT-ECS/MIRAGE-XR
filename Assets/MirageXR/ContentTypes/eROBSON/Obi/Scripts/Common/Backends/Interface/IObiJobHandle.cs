namespace Obi
{
    public interface IObiJobHandle
    {
        void Complete();
        void Release();
    }
}
