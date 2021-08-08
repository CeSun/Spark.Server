using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer
{
    public struct Config
    {
        public MysqlConfig mysqlConfig;
        public RedisConfig redisConfig;
    }

    public struct MysqlConfig
    {
        public string Host;
        public int Port;
        public string Username;
        public string Password;
        public string Database;
        public int PoolSize;
    }

    public struct RedisConfig
    {
        public string Host;
        public int Port;
        public string Username;
        public string Password;
        public int Database;
        public int PoolSize;
    }
}
