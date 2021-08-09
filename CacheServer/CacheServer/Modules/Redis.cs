using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frame;
using StackExchange.Redis;

namespace CacheServer.Modules
{
    public class Redis : Singleton<Redis>
    {
        private IDatabase database;
        public IDatabase Database => database;
        public void Init(RedisConfig config)
        {
            database = ConnectionMultiplexer.Connect(config.Host).GetDatabase(config.Database);
        }

        public void Update()
        {

        }

        public void Fini()
        {
            
        }
    }
}
