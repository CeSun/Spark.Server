using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Frame
{
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        BufferBlock<(SendOrPostCallback d, object? state)> bufferBlock = new BufferBlock<(SendOrPostCallback d, object? state)>();
        public override void Post(SendOrPostCallback d, object? state)
        {
            bufferBlock.Post((d, state));
        }
        public void Update()
        {
            IList<(SendOrPostCallback d, object? state)> list = new List<(SendOrPostCallback d, object? state)>();
            if (bufferBlock.TryReceiveAll(out list))
                foreach (var item in list)
                    item.d?.Invoke(item.state);
        }
    }
}
