using Cacheapi;
using CacheServerApi;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServerApi.Tables
{
    public class TAccount : Table<TAccount, (CacheServerApi.AuthType, string, int), PBAccount>
    {
        public class DirtyKey
        {
            public static readonly string Base = "base";
        }
        protected override string TableName => "DBAccount";
         protected override string GetKey()
        {
            return string.Format("{0}|{1}|{2}", ((int)Base.Type), Base.Account, Base.Zone);
        }

        protected override string GetKey((AuthType, string, int) key)
        {
            return string.Format("{0}|{1}|{2}", ((int)key.Item1), key.Item2, key.Item3);
        }
    }
}
