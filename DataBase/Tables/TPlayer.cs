using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Tables
{
    public class TPlayer : Table<DBPlayer, (int, ulong), TPlayer>
    {
        public override string TableName { get { return "DBPlayer"; } }

        protected override string GetKey()
        {
            return string.Format("{0}|{1}", Value.Zone, Value.Uin);
        }

        protected override string GetKey((int, ulong) key)
        {
            return string.Format("{0}|{1}", key.Item1, key.Item2);
        }


    }
}
