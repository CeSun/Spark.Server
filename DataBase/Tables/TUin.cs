using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Tables
{
    public class TUin : Table<DBUin, int, TUin>
    {
        public override string TableName { get { return "DBUin"; } }

        protected override string GetKey()
        {
            return string.Format("{0}", Value.Zone);
        }

        protected override string GetKey(int key)
        {
            return string.Format("{0}", key);
        }
    }
}
