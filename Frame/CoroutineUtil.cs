using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    public class CoroutineUtil: Singleton<CoroutineUtil>
    {
        SingleThreadSynchronizationContext context;
        public void Init()
        {
            context = SynchronizationContext.Current as SingleThreadSynchronizationContext;
        }


        public void Run(TaskAction action)
        {
            context.PostStart(res => {
                TaskAction f = async () =>
                {
                    try
                    {
                        await action();
                    } catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                };
                f();
            }, null);
        }
    }
}
