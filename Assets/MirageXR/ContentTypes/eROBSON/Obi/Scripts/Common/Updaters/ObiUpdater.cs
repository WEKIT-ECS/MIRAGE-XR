using UnityEngine;
using Unity.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;

using Unity.Jobs;
using Unity.Collections;

namespace Obi
{
    /// <summary>
    /// Base class for updating multiple solvers in parallel.
    /// Derive from this class to write your onw updater. This grants you precise control over execution order,
    /// as you can choose to update solvers at any point during Unity's update cycle.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class ObiUpdater : MonoBehaviour
    {
        static ProfilerMarker m_BeginStepPerfMarker = new ProfilerMarker("BeginStep");
        static ProfilerMarker m_SubstepPerfMarker = new ProfilerMarker("Substep");
        static ProfilerMarker m_EndStepPerfMarker = new ProfilerMarker("EndStep");
        static ProfilerMarker m_InterpolatePerfMarker = new ProfilerMarker("Interpolate");

        /// <summary>
        /// List of solvers updated by this updater.
        /// </summary>
        public List<ObiSolver> solvers = new List<ObiSolver>();

        private List<IObiJobHandle> handles = new List<IObiJobHandle>();


        /// <summary>
        /// Prepares all solvers to begin simulating a new frame. This should be called as soon as possible in the frame,
        /// and guaranteed to be called every frame that will step physics.
        /// </summary>
        protected void PrepareFrame()
        {
            foreach (ObiSolver solver in solvers)
                if (solver != null)
                    solver.PrepareFrame();
        }

        /// <summary>
        /// Prepares all solvers to begin simulating a new physics step. This involves
        /// caching some particle data for interpolation, performing collision detection, among other things.
        /// </summary>
        /// <param name="stepDeltaTime"> Duration (in seconds) of the next step.</param>
        protected void BeginStep(float stepDeltaTime)
        {
            using (m_BeginStepPerfMarker.Auto())
            {
                // Update colliders right before collision detection:
                ObiColliderWorld.GetInstance().UpdateColliders();
                ObiColliderWorld.GetInstance().UpdateRigidbodies(solvers,stepDeltaTime);
                ObiColliderWorld.GetInstance().UpdateWorld(stepDeltaTime);

                handles.Clear();

                // Kick off all solver jobs:
                foreach (ObiSolver solver in solvers)
                    if (solver != null)
                        handles.Add(solver.BeginStep(stepDeltaTime));

                // wait for all solver jobs to complete:
                foreach (IObiJobHandle handle in handles)
                    if (handle != null)
                        handle.Complete();

                foreach (ObiSolver solver in solvers)
                    if (solver != null)
                        solver.ReleaseJobHandles();
            }
        }


        /// <summary>
        /// Advances the simulation a given amount of time. Note that once BeginStep has been called,
        /// Substep can be called multiple times. 
        /// </summary>
        /// <param name="substepDeltaTime"> Duration (in seconds) of the substep.</param>
        protected void Substep(float stepDeltaTime, float substepDeltaTime, int index)
        {
            using (m_SubstepPerfMarker.Auto())
            {
                handles.Clear();

                // Kick off all solver jobs:
                foreach (ObiSolver solver in solvers)
                    if (solver != null)
                        handles.Add(solver.Substep(stepDeltaTime, substepDeltaTime, index));

                // wait for all solver jobs to complete:
                foreach (IObiJobHandle handle in handles)
                    if (handle != null)
                        handle.Complete();

                foreach (ObiSolver solver in solvers)
                    if (solver != null)
                        solver.ReleaseJobHandles();
            }
        }

        /// <summary>
        /// Wraps up the current simulation step. This will trigger contact callbacks.
        /// </summary>
        protected void EndStep(float substepDeltaTime)
        {
            using (m_EndStepPerfMarker.Auto())
            {
                // End step: Invokes collision callbacks and notifies actors that the solver step has ended.
                foreach (ObiSolver solver in solvers)
                    if (solver != null)
                        solver.EndStep(substepDeltaTime);
            }

            // Write back rigidbody velocity deltas:
            ObiColliderWorld.GetInstance().UpdateRigidbodyVelocities(solvers);
        }

        /// <summary>
        /// Interpolates the previous and current physics states. Should be called right before rendering the current frame.
        /// </summary>
        /// <param name="stepDeltaTime"> Duration (in seconds) of the last step taken.</param>
        /// <param name="stepDeltaTime"> Amount of accumulated (not yet simulated) time.</param>
        protected void Interpolate(float stepDeltaTime, float accumulatedTime)
        {
            using (m_InterpolatePerfMarker.Auto())
            {
                foreach (ObiSolver solver in solvers)
                    if (solver != null)
                        solver.Interpolate(stepDeltaTime, accumulatedTime);
            }
        }
    }
}