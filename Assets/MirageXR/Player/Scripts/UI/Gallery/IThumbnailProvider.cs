using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace MirageXR
{
    public interface IThumbnailProvider
    {
        /// <summary>
        /// Loads the thumbnail of an element.
        /// </summary>
        /// <returns>A new texture owned by the caller, who must destroy it when it is no longer shown; or <c>null</c>.</returns>
        UniTask<Texture2D> GetThumbnailAsync(string elementId, CancellationToken cancellationToken);
    }
}
