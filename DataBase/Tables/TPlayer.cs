using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Tables
{
    public class TPlayer : Table<DBPlayer, ulong, TPlayer>
    {
        public override string TableName { get { return "DBPlayer"; } }

        protected override string GetKey()
        {
            return string.Format("{0}", Value.Uin);
        }

        protected override string GetKey(ulong key)
        {
            return string.Format("{0}", key);
        }


    }
}
