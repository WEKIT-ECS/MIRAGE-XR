using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReadyPlayerMe.Core
{
    public class OperationExecutor<T> where T : Context
    {
        private readonly IOperation<T>[] operations;
        private readonly int operationsCount;

        public int Timeout;
        public Action<IOperation<T>> OperationCompleted;

        private CancellationTokenSource ctxSource;
        private int currentIndex;

        private float currentProgress;

        public OperationExecutor(IOperation<T>[] operations)
        {
            this.operations = operations;
            operationsCount = operations.Length;
        }

        public bool IsCancelled => ctxSource.IsCancellationRequested;
        public event Action<float, string> ProgressChanged;

        public async Task<T> Execute(T context)
        {
            ctxSource = new CancellationTokenSource();
            foreach (IOperation<T> operation in operations)
            {
                operation.ProgressChanged += OnProgressChanged;
                operation.Timeout = Timeout;
                try
                {
                    context = await operation.Execute(context, ctxSource.Token);
                    currentIndex++;
                }
                catch
                {
                    if (ctxSource.IsCancellationRequested)
                    {
                        ctxSource.Dispose();
                    }

                    throw;
                }
                operation.ProgressChanged -= OnProgressChanged;
                OperationCompleted?.Invoke(operation);
            }

            return context;
        }

        public void Cancel()
        {
            if (!ctxSource.IsCancellationRequested)
            {
                ctxSource?.Cancel();
            }
        }

        private void OnProgressChanged(float progress)
        {
            currentProgress = 1f / operationsCount * (currentIndex + progress);
            ProgressChanged?.Invoke(currentProgress, operations[currentIndex].GetType().Name);
        }
    }
}
