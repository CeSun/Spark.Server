using Google.Protobuf.WellKnownTypes;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class Database
    {
#pragma warning disable CS8618// 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        private static MySqlConnection mySqlConn;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

#pragma warning disable CS8603 // 可能返回 null 引用。
        public static MySqlConnection MySqlConn { get { if (mySqlConn != null) return mySqlConn; return mySqlConn; } }
#pragma warning restore CS8603 // 可能返回 null 引用。

        static string[] tables;
        public static void Init()
        {
            tables = new string[]{"DBAccount", "DBNickname", "DBPlayer", "DBUin" };
            mySqlConn = new MySqlConnection("Server=localhost;Port=3306;Database=Game01; User=game;Password=1024;sslmode=Required");
            mySqlConn.Open();
            InitTable();
        }
        public static void Update()
        {

        }

        public static void Fini()
        {
            mySqlConn?.Close();
        }
        private static void InitTable()
        {
            var cmd = mySqlConn.CreateCommand();
            cmd.CommandText = "show tables";
            Dictionary<string, byte> dbTables = new Dictionary<string, byte>();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var tablename = reader.GetFieldValue<string>(0);
                dbTables.Add(tablename, 0);
            }
            reader.Close();
            foreach(var table in tables)
            {
                if (dbTables.ContainsKey(table))
                    continue;
                var createCmd = mySqlConn.CreateCommand();
                createCmd.CommandText = string.Format("CREATE TABLE `{0}` (" +
                "`c_key` CHAR(255) NOT NULL COLLATE 'utf8mb4_0900_ai_ci'," +
                "`c_value` LONGBLOB NOT NULL," +
                "`c_version` BIGINT(19) NOT NULL DEFAULT '0'," +
                "PRIMARY KEY(`c_key`) USING BTREE"+
                ")"+
                "COLLATE = 'utf8mb4_0900_ai_ci'"+
                "ENGINE = InnoDB"+
                "; ", table);
                Console.WriteLine(createCmd.CommandText);
                createCmd.ExecuteNonQuery();
            }
        }
    }
}
