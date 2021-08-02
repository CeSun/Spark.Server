using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Tables
{
    public class TAccount : Table<DBAccount, (DataBase.AuthType, string, int), TAccount>
    {
        public override string TableName { get { return "DBAccount"; } }

        protected override string GetKey()
        {
            return string.Format("{0}|{1}|{2}", ((int)Value.Type), Value.Account,Value.Zone);
        }

        protected override string GetKey((AuthType, string, int) key)
        {
            return string.Format("{0}|{1}|{2}", ((int)key.Item1), key.Item2, key.Item3);
        }
    }
}
