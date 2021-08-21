using Cacheapi;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ProxyServerApi.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServerApi.Tables
{
    public class TPlayer : Table<TPlayer, (int, ulong), PBPlayer>
    {
        public class DirtyKey
        {
            public static readonly string Base = "base";
        }
        protected override string TableName => "DBPlayer";

        protected override string GetKey()
        {
            return string.Format("{0}|{1}", Base.Zone, Base.Uin);
        }

        protected override string GetKey((int, ulong) key)
        {
            return string.Format("{0}|{1}", key.Item1, key.Item2);

        }
    }
}
