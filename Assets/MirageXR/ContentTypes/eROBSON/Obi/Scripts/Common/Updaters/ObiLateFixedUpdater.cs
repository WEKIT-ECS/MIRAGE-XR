using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Obi
{
    /// <summary>
    /// Updater class that will perform simulation after WaitForFixedUpdate. Use this for simulations that require animation data as input, such as character clothing.
    /// Make sure to set the Animator update mode to "Animate Physics".
                  
    [AddComponentMenu("Physics/Obi/Obi Late Fixed Updater", 802)]
    [ExecuteInEditMode]
    public class ObiLateFixedUpdater : ObiUpdater
    {

        /// <summary>
        /// Each LateFixedUpdate() call will be divided into several substeps. Performing more substeps will greatly improve the accuracy/convergence speed of the simulation. 
        /// Increasing the amount of substeps is more effective than increasing the amount of constraint iterations.
        /// </summary>
        [Tooltip("Amount of substeps performed per FixedUpdate. Increasing the amount of substeps greatly improves accuracy and convergence speed.")]
        public int substeps = 4;

        [NonSerialized] private float accumulatedTime;

        private void OnValidate()
        {
            substeps = Mathf.Max(1, substeps);
        }

        private void OnEnable()
        {
            accumulatedTime = 0;
            StartCoroutine(RunLateFixedUpdate());
        }
        private void OnDisable()
        {
            StopCoroutine(RunLateFixedUpdate());
        }

        private void FixedUpdate()
        {
            PrepareFrame();
        }

        private IEnumerator RunLateFixedUpdate()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (Application.isPlaying)
                    LateFixedUpdate();
            }
        }

        private void LateFixedUpdate()
        {
            ObiProfiler.EnableProfiler();

            BeginStep(Time.fixedDeltaTime);

            float substepDelta = Time.fixedDeltaTime / (float)substeps;

            // Divide the step into multiple smaller substeps:
            for (int i = 0; i < substeps; ++i)
                Substep(Time.fixedDeltaTime, substepDelta, substeps - i);

            EndStep(substepDelta);

            ObiProfiler.DisableProfiler();

            accumulatedTime -= Time.fixedDeltaTime;
        }

        private void Update()
        {
            accumulatedTime += Time.deltaTime;

            ObiProfiler.EnableProfiler();
            Interpolate(Time.fixedDeltaTime, accumulatedTime);
            ObiProfiler.DisableProfiler();
        }
    }
}