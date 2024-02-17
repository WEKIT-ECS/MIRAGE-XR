using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Obi
{
    /// <summary>
    /// Updater class that will perform simulation during FixedUpdate(). This is the most physically correct updater,
    /// and the one to be used in most cases. Also allows to perform substepping, greatly improving convergence.
                  
    [AddComponentMenu("Physics/Obi/Obi Fixed Updater", 801)]
    [ExecuteInEditMode]
    public class ObiFixedUpdater : ObiUpdater
    {
        /// <summary>
        /// Each FixedUpdate() call will be divided into several substeps. Performing more substeps will greatly improve the accuracy/convergence speed of the simulation. 
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
        }

        private void OnDisable()
        {
            Physics.autoSimulation = true;
        }

        private void FixedUpdate()
        {
            ObiProfiler.EnableProfiler();

            PrepareFrame();

            BeginStep(Time.fixedDeltaTime);

            float substepDelta = Time.fixedDeltaTime / (float)substeps;

            // Divide the step into multiple smaller substeps:
            for (int i = 0; i < substeps; ++i)
                Substep(Time.fixedDeltaTime, substepDelta, substeps-i);

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