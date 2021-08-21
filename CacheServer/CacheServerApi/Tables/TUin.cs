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
    public class TUin : Table<TUin, int, PBUin>
    {
        public class DirtyKey
        {
            public static readonly string Base = "base";
        }
        protected override string TableName => "DBUin";

        protected override string GetKey()
        {
            return string.Format("{0}", Base.Zone);
        }

        protected override string GetKey(int key)
        {
            return string.Format("{0}", key);
        }
    }
}
