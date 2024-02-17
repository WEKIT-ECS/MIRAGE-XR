using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
    /**
     * General greedy graph coloring algorithm for constraints. Input:
     * - List of particle indices used by all constraints.
     * - List of per-constraint offsets of the first constrained particle in the previous array, with the total amount of particle indices in the last position.
     * 
     * The output is a color for each constraint. Constraints of the same color are guaranteed to not share any partices.
     */
    public class GraphColoring
    {
        private List<int> m_ParticleIndices;
        private List<int> m_ConstraintIndices;
        private List<int>[] m_ConstraintsPerParticle;

        public IReadOnlyList<int> particleIndices => m_ParticleIndices.AsReadOnly();
        public IReadOnlyList<int> constraintIndices => m_ConstraintIndices.AsReadOnly();

        public GraphColoring(int particleCount)
        {
            m_ParticleIndices = new List<int>();
            m_ConstraintIndices = new List<int>();
            m_ConstraintsPerParticle = new List<int>[particleCount];
            for (int i = 0; i < m_ConstraintsPerParticle.Length; ++i)
                m_ConstraintsPerParticle[i] = new List<int>();
        }

        public void Clear()
        {
            m_ParticleIndices.Clear();
            m_ConstraintIndices.Clear();
            for (int i = 0; i < m_ConstraintsPerParticle.Length; ++i)
                m_ConstraintsPerParticle[i].Clear();
        }

        public void AddConstraint(int[] particles)
        {
            for (int i = 0; i < particles.Length; ++i)
                m_ConstraintsPerParticle[particles[i]].Add(m_ConstraintIndices.Count);

            m_ConstraintIndices.Add(m_ParticleIndices.Count);
            m_ParticleIndices.AddRange(particles);
        }

        public IEnumerator Colorize(string progressDescription, List<int> colors)
        {
            m_ConstraintIndices.Add(m_ParticleIndices.Count);

            int constraintCount = Mathf.Max(0, m_ConstraintIndices.Count - 1);
            colors.Clear();

            if (constraintCount == 0)
                yield break;

            colors.Capacity = constraintCount;
            bool[] availability = new bool[constraintCount];

            for (int i = 0; i < constraintCount; ++i)
            {
                colors.Add(-1);
                availability[i] = true;
            }

            // For each constraint:
            for (int i = 0; i < constraintCount; ++i)
            {
                // iterate over its particles:
                for (int j = m_ConstraintIndices[i]; j < m_ConstraintIndices[i + 1]; ++j)
                {
                    // for each particle, get constraints affecting it:
                    foreach (int k in m_ConstraintsPerParticle[m_ParticleIndices[j]])
                    {
                        // skip ourselves:
                        if (i == k) continue;

                        // both constraints share a particle so mark the neighbour color as unavailable:
                        if (colors[k] >= 0)
                            availability[colors[k]] = false;
                    }
                }

                // Assign the first available color:
                for (colors[i] = 0; colors[i] < constraintCount; ++colors[i])
                    if (availability[colors[i]])
                        break;

                // Reset availability flags:
                for (int j = 0; j < constraintCount; ++j)
                    availability[j] = true;

                yield return new CoroutineJob.ProgressInfo(progressDescription, i / (float)constraintCount);
            }
        }


    }
}
