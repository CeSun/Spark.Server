using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Tables
{
    public class TNickname : Table<DBNickname, string, TNickname>
    {
        public override string TableName { get { return "DBNickname"; } }

        protected override string GetKey()
        {
            return Value.Nickname;
        }

        protected override string GetKey(string key)
        {
            return key;
        }
    }
}
