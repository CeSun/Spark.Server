using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public abstract class Pool<TConnector, TConfig, TSub>: Singleton<TSub> where TSub: Pool<TConnector, TConfig, TSub>, new ()
    {
        protected Stack<TConnector> connectors = new Stack<TConnector>();
        public abstract void Init(TConfig config);
        public PoolMeta Borrow()
        {
            TConnector connector;
            if (connectors.TryPop(out connector))
            {
                return new PoolMeta(connector);
            }
            return null;
        }
        private void Return(ref TConnector Connection)
        {
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
