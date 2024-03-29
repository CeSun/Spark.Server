﻿using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CacheServer
{
    public class Config : BaseConfig
    {
        public MysqlConfig Mysql;
        public RedisConfig Redis;
        public CacheServerConf CacheServer;
        [XmlArray("IpAndPoint"), XmlArrayItem("value")]
        public string[] IpAndPoint;
    }
    public struct CacheServerConf
    {
        public int SaveInterval;
        
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
    }
}
