using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        enum Source
        {
            Task,
            Start
        }
        public SingleThreadSynchronizationContext()
        {
        }
        BlockingCollection<(SendOrPostCallback d, object state, Source)> bufferBlock = new BlockingCollection<(SendOrPostCallback d, object state, Source)>();
        public override void Post(SendOrPostCallback d, object state)
        {
            bufferBlock.Add((d, state, Source.Task));
        }
        public void PostStart(SendOrPostCallback d, object state)
        {
            bufferBlock.Add((d, state, Source.Start));
        }
        public void Update()
        {
            (SendOrPostCallback d, object state, Source) data = default;
            int i = 0;
            for (i = 0; i < 10 ; i++)
            {
                if (!bufferBlock.TryTake(out data))
                    break;
                data.d(data.state);
            }
        }
    }
}
