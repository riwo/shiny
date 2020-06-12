using System.Threading;
using System.Threading.Tasks;

namespace Shiny.Infrastructure
{
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync() => this.tcs.Task;

        public void Set() => this.tcs.TrySetResult(true);

        public void Reset()
        {
            while (true)
            {
                var tcs = this.tcs;
                if (!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref this.tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                    return;
            }
        }
    }
}
