using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frame;
using StackExchange.Redis;

namespace CacheServer.Modules
{
    public class Redis : Pool<IDatabase,RedisConfig ,Redis>
    {
        public override void Init(RedisConfig config)
        {
            for (var i = 0; i < config.PoolSize; i ++)
            {
                connectors.Push(ConnectionMultiplexer.Connect(config.Host).GetDatabase(config.Database));
            }
        }

        public void Update()
        {

        }

        public void Fini()
        {
            
        }
    }
}
