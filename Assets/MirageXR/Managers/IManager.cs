using Cysharp.Threading.Tasks;

namespace MirageXR
{
    public interface IManager
    {
        bool IsInitialized { get; }
        UniTask WaitForInitialization();
    }
}