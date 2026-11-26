using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR.FileCache
{
    /// <summary>
    /// Loads images from a <see cref="CachedFileProvider"/> into textures. Decoding is done by
    /// <see cref="UnityWebRequestTexture"/> off the main thread; textures are non-readable (no CPU-side copy).
    /// </summary>
    public static class CachedTextureLoader
    {
        /// <summary>
        /// Returns a new texture owned by the caller (destroy it when it is no longer shown), or <c>null</c>.
        /// </summary>
        public static Task<Texture2D> LoadAsync(CachedFileProvider provider, CacheRequest request, CancellationToken cancellationToken = default)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return provider.LoadAsync(request, LoadTextureFileAsync, cancellationToken);
        }

        private static async Task<Texture2D> LoadTextureFileAsync(string path, CancellationToken cancellationToken)
        {
            using var webRequest = UnityWebRequestTexture.GetTexture(new Uri(path).AbsoluteUri, true);
            try
            {
                await webRequest.SendWebRequest().WithCancellation(cancellationToken);
            }
            catch (UnityWebRequestException)
            {
                // evaluated below
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[CachedTextureLoader] could not decode '{path}': {webRequest.error}");
                return null;
            }

            var texture = DownloadHandlerTexture.GetContent(webRequest);
            if (texture != null && cancellationToken.IsCancellationRequested)
            {
                UnityEngine.Object.Destroy(texture);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return texture;
        }
    }
}
