using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frame;
using MySqlConnector;

namespace CacheServer.Modules
{
    public class Mysql : ConnectPool<MySqlConnection, MysqlConfig, Mysql>
    {
        string connectStr = "";
        public override void Init(MysqlConfig mysqlConfig)
        {
            connectStr = string.Format("Server={0};Port={1};Database={2}; User={3};Password={4};sslmode=Required", mysqlConfig.Host, mysqlConfig.Port, mysqlConfig.Database, mysqlConfig.Username, mysqlConfig.Password);
            for (int i = 0; i < mysqlConfig.PoolSize; i ++)
            {
                MySqlConnection mySqlConnection = new MySqlConnection(connectStr);
                mySqlConnection.Open();
                connectors.Push(mySqlConnection);
            }
        }

        public void Update()
        {

        }

        public void Fini()
        {

        }

        public override async Task NewAsync(int num)
        {
            for (int i = 0; i < num; i++)
            {
                MySqlConnection mySqlConnection = new MySqlConnection (connectStr);
                await mySqlConnection.OpenAsync();
                connectors.Push(mySqlConnection);
            }
        }
    }
}
