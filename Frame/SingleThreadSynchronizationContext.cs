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
        int mainThreadId;
        enum Source
        {
            Task,
            Start
        }
        public SingleThreadSynchronizationContext()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
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
            Stopwatch sw = Stopwatch.StartNew();
            (SendOrPostCallback d, object state, Source) data = default;
            for (int i = 0; i < 10 ; i++)
            {
                if (!bufferBlock.TryTake(out data))
                    break;
                Stopwatch sw2 = Stopwatch.StartNew();
                data.d(data.state);
                sw2.Stop();
                if (data.Item3 == Source.Start)
                {

                }
            }
            sw.Stop();
        }
    }
}
