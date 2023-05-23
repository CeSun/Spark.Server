using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private int ThreadId = default;
        BlockingCollection<(SendOrPostCallback, object?)> _Queue = new BlockingCollection<(SendOrPostCallback, object?)> ();
        public void Init()
        {
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            SetSynchronizationContext(this);
        }
        public override void Post(SendOrPostCallback d, object? state)
        {
            _Queue.Add((d, state));
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            base.Send(d, state);
        }

        public void Update()
        {
            foreach(var (callback, state) in _Queue.GetConsumingEnumerable())
            {
                callback(state);
            }
        }

    }
}
