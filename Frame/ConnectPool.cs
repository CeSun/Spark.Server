using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TConnector"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    /// <typeparam name="TSub"></typeparam>
    public abstract class ConnectPool<TConnector, TConfig, TSub>: Singleton<TSub> where TSub: ConnectPool<TConnector, TConfig, TSub>, new ()
    {
        protected Stack<TConnector> connectors = new Stack<TConnector>();
        public abstract void Init(TConfig config);
        public abstract Task NewAsync(int num);
        private Queue<TaskCompletionSource<PoolMeta>> tcss = new Queue<TaskCompletionSource<PoolMeta>>();
        public PoolMeta Borrow()
        {
            TConnector connector;
            if (connectors.TryPop(out connector))
                return new PoolMeta(connector);
            return null;
        }

        public Task<PoolMeta> BorrowAsync()
        {
            TaskCompletionSource<PoolMeta> tcs = new TaskCompletionSource<PoolMeta>();
            var meta = Borrow();
            if (meta != null)
                tcs.SetResult(meta);
            else
                tcss.Enqueue(tcs);
            return tcs.Task;
        }
        private void Return(ref TConnector Connection)
        {
            TaskCompletionSource<PoolMeta> tcs;
            if (tcss.TryDequeue(out tcs))
                tcs.SetResult(new PoolMeta(Connection));
            else
                connectors.Push(Connection);
            Connection = default;
        }
        public class PoolMeta : IDisposable
        {
            public TConnector Connector => connector;

            private TConnector connector;
            internal PoolMeta(TConnector connector)
            {
                this.connector = connector;
            }
            public void Dispose()
            {
                Instance.Return(ref connector);
            }
        }
    }
    

}
