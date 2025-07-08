using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReadyPlayerMe.Core
{
    public abstract class Context
    {
    }

    public interface IOperation<T> where T : Context
    {
        int Timeout { get; set; }
        Action<float> ProgressChanged { get; set; }
        Task<T> Execute(T context, CancellationToken token);
    }
}
