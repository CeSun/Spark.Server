using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Frame
{
    public abstract class Application
    {
        public BufferBlock<ESingle> Single = new BufferBlock<ESingle>();
        protected abstract Task OnInitAsync();
        protected abstract Task OnFiniAsync();
        public async Task RunAsync()
        {
            await OnInitAsync();
            bool isRun = true;
            while (isRun)
            {
                var single = await Single.ReceiveAsync();
                switch (single)
                {
                    case ESingle.Exception:
                    case ESingle.Exit:
                        isRun = false;
                        break;
                    default:
                        break;
                }
            }
            await OnFiniAsync();
        }
    }

    public enum ESingle
    {
        Exit,
        Exception
    }
}
