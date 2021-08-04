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
            // 如果是主线程，直接执行，其他线程写到队列里
            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (threadId == mainThreadId)
                d?.Invoke(state);
            else
                bufferBlock.Add((d, state));
        }

        public void Update()
        {
            IList<(SendOrPostCallback d, object state)> list = new List<(SendOrPostCallback d, object state)>();
            for (int i = 0; i < 10; i++ )
            {
                (SendOrPostCallback d, object state) data = default;
                if (!bufferBlock.TryTake(out data))
                {
                    break;
                }
                data.d?.Invoke(data.state);
            }
        }
    }
}
