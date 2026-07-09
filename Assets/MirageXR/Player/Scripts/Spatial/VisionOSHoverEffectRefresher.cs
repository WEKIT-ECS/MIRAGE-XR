using System.Collections;
using Unity.PolySpatial;
using UnityEngine;

namespace MirageXR
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VisionOSHoverEffect))]
    public sealed class VisionOSHoverEffectRefresher : MonoBehaviour
    {
        private VisionOSHoverEffect _hoverEffect;
        private Coroutine _refreshRoutine;

        private void Awake()
        {
            _hoverEffect = GetComponent<VisionOSHoverEffect>();
        }

        private void OnEnable()
        {
            _refreshRoutine = StartCoroutine(RefreshHoverEffect());
        }

        private void OnDisable()
        {
            if (_refreshRoutine == null)
            {
                return;
            }

            StopCoroutine(_refreshRoutine);
            _refreshRoutine = null;
        }

        private IEnumerator RefreshHoverEffect()
        {
            yield return null;

            if (_hoverEffect == null)
            {
                yield break;
            }

            _hoverEffect.IntensityMultiplier++;

            yield return null;

            if (_hoverEffect != null)
            {
                _hoverEffect.IntensityMultiplier--;
            }

            _refreshRoutine = null;
        }
    }
}
