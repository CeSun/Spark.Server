using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public abstract class Pool<TConnector, TConfig, TSub>: Singleton<TSub> where TSub : new ()
    {
        protected Stack<TConnector> connectors = new Stack<TConnector>();
        public abstract void Init(TConfig config);
        public PoolMeta<TConnector, TConfig, TSub> Borrow()
        {
            TConnector connector;
            if (connectors.TryPop(out connector))
            {
                return new PoolMeta<TConnector, TConfig, TSub>(this, connector);
            }
            return null;
        }
        internal void Return(ref TConnector Connection)
        {
            connectors.Push(Connection);
            Connection = default;
        }
    }
    public class PoolMeta<TConnector, TConfig, TSub> : IDisposable where TSub : new()
    {
        Pool<TConnector, TConfig, TSub> Pool;
        public TConnector Connector => connector;

        private TConnector connector;
        internal PoolMeta(Pool<TConnector, TConfig, TSub> pool, TConnector connector)
        {
            Pool = pool;
            this.connector = connector;
        }
        public void Dispose()
        {
            Pool.Return(ref connector);
        }
    }

}
