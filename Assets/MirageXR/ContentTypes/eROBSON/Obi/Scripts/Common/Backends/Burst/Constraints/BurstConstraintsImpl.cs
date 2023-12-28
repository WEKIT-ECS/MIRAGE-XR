﻿#if (OBI_BURST && OBI_MATHEMATICS && OBI_COLLECTIONS)
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
    public interface IBurstConstraintsImpl : IConstraints
    {
        JobHandle Initialize(JobHandle inputDeps, float substepTime);
        JobHandle Project(JobHandle inputDeps, float stepTime, float substepTime, int substeps);
        void Dispose();

        IConstraintsBatchImpl CreateConstraintsBatch();
        void RemoveBatch(IConstraintsBatchImpl batch);
    }

    public abstract class BurstConstraintsImpl<T> : IBurstConstraintsImpl where T : BurstConstraintsBatchImpl
    {
        protected BurstSolverImpl m_Solver;
        public List<T> batches = new List<T>();

        protected Oni.ConstraintType m_ConstraintType;

        public Oni.ConstraintType constraintType
        {
            get { return m_ConstraintType; }
        }

        public ISolverImpl solver
        {
            get { return m_Solver; }
        }

        public BurstConstraintsImpl(BurstSolverImpl solver, Oni.ConstraintType constraintType)
        {
            this.m_ConstraintType = constraintType;
            this.m_Solver = solver;
        }

        public virtual void Dispose()
        {

        }

        public abstract IConstraintsBatchImpl CreateConstraintsBatch();


        public abstract void RemoveBatch(IConstraintsBatchImpl batch);


        public virtual int GetConstraintCount()
        {
            int count = 0;
            if (batches == null) return count;

            foreach (T batch in batches)
                if (batch != null)
                    count += batch.GetConstraintCount();

            return count;
        }

        public JobHandle Initialize(JobHandle inputDeps, float substepTime)
        {
            // initialize all batches in parallel:
            if (batches.Count > 0)
            {
                NativeArray<JobHandle> deps = new NativeArray<JobHandle>(batches.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < batches.Count; ++i)
                    deps[i] = batches[i].enabled ? batches[i].Initialize(inputDeps, substepTime) : inputDeps;

                JobHandle result = JobHandle.CombineDependencies(deps);
                deps.Dispose();

                return result;
            }

            return inputDeps;
        }

        public JobHandle Project(JobHandle inputDeps, float stepTime, float substepTime, int substeps)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Project");

            var parameters = m_Solver.abstraction.GetConstraintParameters(m_ConstraintType);

            switch(parameters.evaluationOrder)
            {
                case Oni.ConstraintParameters.EvaluationOrder.Sequential:
                    inputDeps = EvaluateSequential(inputDeps, stepTime, substepTime, substeps);
                break;

                case Oni.ConstraintParameters.EvaluationOrder.Parallel:
                    inputDeps = EvaluateParallel(inputDeps, stepTime, substepTime, substeps);
                break;
            }

            UnityEngine.Profiling.Profiler.EndSample();
            
            return inputDeps;
        }

        protected virtual JobHandle EvaluateSequential(JobHandle inputDeps, float stepTime, float substepTime,int substeps)
        {
            // evaluate and apply all batches:
            for (int i = 0; i < batches.Count; ++i)
            {
                if (batches[i].enabled)
                {
                    inputDeps = batches[i].Evaluate(inputDeps, stepTime, substepTime, substeps);
                    inputDeps = batches[i].Apply(inputDeps, substepTime);
                    m_Solver.ScheduleBatchedJobsIfNeeded();
                }
            }

            return inputDeps;
        }

        protected virtual JobHandle EvaluateParallel(JobHandle inputDeps, float stepTime, float substepTime, int substeps)
        {
            // evaluate all batches:
            for (int i = 0; i < batches.Count; ++i)
                if (batches[i].enabled)
                {
                    inputDeps = batches[i].Evaluate(inputDeps, stepTime, substepTime, substeps);
                    m_Solver.ScheduleBatchedJobsIfNeeded();
                }

            // then apply them:
            for (int i = 0; i < batches.Count; ++i)
                if (batches[i].enabled)
                {
                    inputDeps = batches[i].Apply(inputDeps, substepTime);
                    m_Solver.ScheduleBatchedJobsIfNeeded();
                }

            return inputDeps;
        }

    }
}
#endif