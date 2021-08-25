using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    public delegate Task CoroutineAction();
    public class CoroutineUtil: Singleton<CoroutineUtil>
    {
        SingleThreadSynchronizationContext context;
        public void Init()
        {
            context = SynchronizationContext.Current as SingleThreadSynchronizationContext;
        }
        public void New(Action action)
        {
            SendOrPostCallback f = obj =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            };
            //f(null);
            context.PostStart(f, null);
        }

        public void New(CoroutineAction action)
        {
            SendOrPostCallback f = async obj =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            };
            f(null);
            // context.PostStart(f, null);
        }
    }
}
