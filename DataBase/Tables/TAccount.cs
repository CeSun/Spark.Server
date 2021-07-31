using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Tables
{
    public class TAccount : Table<DBAccount, (DataBase.AuthType, string), TAccount>
    {
        public override string TableName { get { return "DBAccount"; } }

        protected override string GetKey()
        {
            return string.Format("{0}|{1}", Value.Type, Value.Account);
        }

        protected override string GetKey((AuthType, string) key)
        {
            return string.Format("{0}|{1}", key.Item1, key.Item2);
        }
    }
}
