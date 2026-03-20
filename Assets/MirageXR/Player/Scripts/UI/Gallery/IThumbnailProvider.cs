using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MirageXR
{
    public interface IThumbnailProvider
    {
        UniTask<Texture2D> GetThumbnailAsync(string elementId, CancellationToken cancellationToken);
    }
}
