﻿using Google.Protobuf.WellKnownTypes;
using Microsoft.VisualBasic;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public struct MysqlConfig
    {
        public string Host;
        public string Username;
        public string Password;
        public string Database;
        public int Port;
    }
    public class Database
    {

        private static string connectStr = "";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectStr);
        }

        static string[] tables= new string[0];

        public static void Init(MysqlConfig Mysql)
        {
            tables = new string[]{"DBAccount", "DBNickname", "DBPlayer", "DBUin" };
            connectStr = string.Format("Server={0};Port={1};Database={2}; User={3};Password={4};sslmode=Required", Mysql.Host, Mysql.Port, Mysql.Database, Mysql.Username, Mysql.Password);
            InitTable();
        }
        public static void Update()
        {

        }

        public static void Fini()
        {
        }
        private static void InitTable()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "show tables";
                Dictionary<string, byte> dbTables = new Dictionary<string, byte>();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var tablename = reader.GetFieldValue<string>(0);
                    dbTables.Add(tablename, 0);
                }
                reader.Close();
                foreach (var table in tables)
                {
                    if (dbTables.ContainsKey(table))
                        continue;
                    var createCmd = conn.CreateCommand();
                    createCmd.CommandText = string.Format("CREATE TABLE `{0}` (" +
                    "`c_key` CHAR(255) NOT NULL ," +
                    "`c_value` LONGBLOB NOT NULL," +
                    "`c_version` BIGINT(19) NOT NULL DEFAULT '0'," +
                    "PRIMARY KEY(`c_key`) USING BTREE" +
                    ")" +
                    "ENGINE = InnoDB" +
                    "; ", table);
                    Console.WriteLine(createCmd.CommandText);
                    createCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
