using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Obi
{
    /// <summary>
    /// Updater class that will perform simulation during LateUpdate(). This is highly unphysical and should be avoided whenever possible.
    /// This updater does not make any accuracy guarantees when it comes to two-way coupling with rigidbodies.
    /// It is only provided for the odd case when there's no way to perform simulation with a fixed timestep.
    /// If in doubt, use the ObiFixedUpdater component instead.
    /// </summary>
    [AddComponentMenu("Physics/Obi/Obi Late Updater", 802)]
    public class ObiLateUpdater : ObiUpdater
    {
        /// <summary>
        /// Smoothing factor fo the timestep (smoothDelta). Values closer to 1 will yield stabler simulation, but it will be off-sync with rendering.
        /// </summary>
        [Tooltip("Smoothing factor fo the timestep (smoothDelta). Values closer to 1 will yield stabler simulation, but it will be off-sync with rendering.")]
        [Range(0,1)]
        public float deltaSmoothing = 0.95f;

        /// <summary>
        /// Target timestep used to advance the simulation. The updater will interpolate this value with Time.deltaTime to find the actual timestep used for each frame.
        /// </summary>
        [Tooltip("Target timestep used to advance the simulation. The updater will interpolate this value with Time.deltaTime to find the actual timestep used for each frame.")]
        private float smoothDelta = 0.02f;

        /// <summary>
        /// Each FixedUpdate() call will be divided into several substeps. Performing more substeps will greatly improve the accuracy/convergence speed of the simulation. 
        /// Increasing the amount of substeps is more effective than increasing the amount of constraint iterations.
        /// </summary>
        [Tooltip("Amount of substeps performed per FixedUpdate. Increasing the amount of substeps greatly improves accuracy and convergence speed.")]
        public int substeps = 4;

        private void OnValidate()
		{
            smoothDelta = Mathf.Max(0.0001f, smoothDelta);
            substeps = Mathf.Max(1, substeps);
        }

        private void Update()
        {
            PrepareFrame();
        }

        private void LateUpdate()
        {
            if (Time.deltaTime > 0)
            {
                if (Application.isPlaying)
                {
                    // smooth out timestep:
                    smoothDelta = Mathf.Lerp(Time.deltaTime, smoothDelta, deltaSmoothing);

                    BeginStep(smoothDelta);

                    float substepDelta = smoothDelta / (float)substeps;

                    // Divide the step into multiple smaller substeps:
                    for (int i = 0; i < substeps; ++i)
                        Substep(smoothDelta, substepDelta, substeps - i);

                    EndStep(substepDelta);
                }

                Interpolate(smoothDelta, smoothDelta);
            }
        }
    }
}