using CacheServerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer.Tables
{
    class TBUin : Table<TBUin>
    {
        Dictionary<string, string> fields = new Dictionary<string, string>
        {
            { "c_base", "base"}
        };

        protected override Dictionary<string, string> Fields => fields;

        protected override string TableName => "DBUin";

    }
}
