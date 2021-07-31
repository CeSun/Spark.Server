using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class Database
    {
        private static MySqlConnection? mySqlConn;
#pragma warning disable CS8603 // 可能返回 null 引用。
        public static MySqlConnection MySqlConn { get { if (mySqlConn != null) return mySqlConn; return mySqlConn; } }
#pragma warning restore CS8603 // 可能返回 null 引用。
        public static void Init()
        {
            mySqlConn = new MySqlConnection("Server=localhost;Port=3306;Database=Game01; User=game;Password=1024;sslmode=Required");
            mySqlConn.Open();
        }

        public static void Update()
        {

        }

        public static void Fini()
        {
            mySqlConn?.Close();
        }
    }
}
