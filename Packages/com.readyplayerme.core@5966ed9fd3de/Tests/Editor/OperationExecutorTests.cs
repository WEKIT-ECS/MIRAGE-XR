using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ReadyPlayerMe.Core.Tests
{
    public class OperationExecutorTests
    {
        private class DummyContext : Context
        {
            public bool IsValueChanged;
        }

        private class DummyOperation : IOperation<DummyContext>
        {
            public int Timeout { get; set; }
            public Action<float> ProgressChanged { get; set; }

            public Task<DummyContext> Execute(DummyContext context, CancellationToken token)
            {
                ProgressChanged?.Invoke(1);
                context.IsValueChanged = true;
                return Task.FromResult(context);
            }
        }

        private class DummyOperationWithTimeout : IOperation<DummyContext>
        {
            public int Timeout { get; set; }
            public Action<float> ProgressChanged { get; set; }

            public async Task<DummyContext> Execute(DummyContext context, CancellationToken token)
            {
                var sw = Stopwatch.StartNew();
                var elapsedTime = 0f;
                while (elapsedTime < Timeout && !token.IsCancellationRequested)
                {
                    elapsedTime = sw.ElapsedMilliseconds / 1000f;
                    await Task.Yield();
                    ProgressChanged?.Invoke(elapsedTime / Timeout);
                }

                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                ProgressChanged?.Invoke(1);
                return context;
            }
        }

        [Test]
        public async Task Executes_Without_Error()
        {
            var executor = new OperationExecutor<DummyContext>(new IOperation<DummyContext>[]
            {
                new DummyOperation()
            });

            Context resultContext;

            try
            {
                resultContext = await executor.Execute(new DummyContext());
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.IsNotNull(resultContext);
            Assert.Pass();
        }

        [Test]
        public async Task Context_Has_Value_Changed_After_Execution()
        {
            var executor = new OperationExecutor<DummyContext>(new IOperation<DummyContext>[]
            {
                new DummyOperation()
            });

            var context = new DummyContext();

            try
            {
                context = await executor.Execute(context);
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.AreEqual(true, context.IsValueChanged);
        }

        [Test]
        public async Task ProgressChanged_Have_Correct_Values_On_Complete()
        {
            var progress = 0f;
            var operation = string.Empty;

            var executor = new OperationExecutor<DummyContext>(new IOperation<DummyContext>[]
            {
                new DummyOperation()
            });

            executor.ProgressChanged += (prog, op) =>
            {
                operation = op;
                progress = prog;
            };
            try
            {
                await executor.Execute(new DummyContext());
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.AreEqual(nameof(DummyOperation), operation);
            Assert.AreEqual(1, progress);
        }

        [Test]
        public async Task Execution_Timeouts()
        {
            var executor = new OperationExecutor<DummyContext>(new IOperation<DummyContext>[]
            {
                new DummyOperationWithTimeout()
            });

            executor.Timeout = 2;

            try
            {
                await executor.Execute(new DummyContext());
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
                throw;
            }

            Assert.Pass();
        }

        [Test]
        public async Task Execution_Is_Cancelled()
        {
            var executor = new OperationExecutor<DummyContext>(new IOperation<DummyContext>[]
            {
                new DummyOperationWithTimeout()
            });

            var progress = 0f;
            var cancelThreshold = 0.05f;

            executor.Timeout = 20;
            executor.ProgressChanged += (prog, operation) =>
            {
                progress = prog;
                if (progress > cancelThreshold)
                {
                    executor.Cancel();
                }
            };

            try
            {
                await executor.Execute(new DummyContext());
            }
            catch (Exception exception)
            {
                if (!executor.IsCancelled)
                {
                    Assert.Fail(exception.Message);
                    throw;
                }

            }

            Assert.IsTrue(executor.IsCancelled);
            Assert.IsTrue(progress > cancelThreshold);
            Assert.Pass();
        }
    }
}
