using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Frame
{
    /// <summary>
    /// 单线程调度异步的对象
    /// </summary>
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        int mainThreadId;
        public SingleThreadSynchronizationContext()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
        BlockingCollection<(SendOrPostCallback d, object state)> bufferBlock = new BlockingCollection<(SendOrPostCallback d, object state)>();
        public override void Post(SendOrPostCallback d, object state)
        {
            bufferBlock.Add((d, state));
        }

        public void Update()
        {
            (SendOrPostCallback d, object state) data = default;
            for (int i = 0; i < 20 ; i++)
            {
                if (!bufferBlock.TryTake(out data))
                    break;
                // data.d(data.state);
            }
        }
    }
}
