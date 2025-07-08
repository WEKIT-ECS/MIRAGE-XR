using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReadyPlayerMe
{
    public static class TransformExtensions
    {
        public static async Task LerpScale(this Transform transform, Vector3 targetScale, float duration, CancellationToken token)
        {
            var time = 0f;
            var startScale = transform.localScale;
            while (time < duration && !token.IsCancellationRequested)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
                time += Time.deltaTime;
                await Task.Yield();
            }

            if (!token.IsCancellationRequested)
            {
                transform.localScale = targetScale;
            }
        }

        public static async Task LerpPosition(this Transform transform, Vector3 targetPosition, float duration, CancellationToken token)
        {
            var time = 0f;
            var startPosition = transform.position;
            while (time < duration && !token.IsCancellationRequested)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                await Task.Yield();
            }

            if (!token.IsCancellationRequested)
            {
                transform.position = targetPosition;
            }
        }
    }
}
