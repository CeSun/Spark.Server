using Cacheapi;
using Google.Protobuf;
using ProxyServerApi.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServerApi.Tables
{
    public class TNickname : Table<TNickname, string, PBNickname>
    {
        public class DirtyKey
        {
            public static readonly string Base = "base";
        }
        protected override string TableName => "DBNickname";

        protected override string GetKey()
        {
            return Base.Nickname;
        }

        protected override string GetKey(string key)
        {
            return key;
        }
    }
}
