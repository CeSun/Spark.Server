using CacheServer.Modules;
using CacheServerApi;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer.Tables
{
    class TBPlayer : Table<TBPlayer>
    {

        Dictionary<string, string> fields = new Dictionary<string, string>()
            {
                {"c_base", "base"},
                {"c_currency", "currency"},
            };
        protected override Dictionary<string, string> Fields => fields;

        protected override string TableName => "DBPlayer";

    }
}
