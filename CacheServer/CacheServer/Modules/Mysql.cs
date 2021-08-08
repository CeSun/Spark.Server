using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frame;
using MySqlConnector;

namespace CacheServer.Modules
{
    public class Mysql : Pool<MySqlConnection, MysqlConfig, Mysql>
    {
        public override void Init(MysqlConfig mysqlConfig)
        {
            var connectStr = string.Format("Server={0};Port={1};Database={2}; User={3};Password={4};sslmode=Required", mysqlConfig.Host, mysqlConfig.Port, mysqlConfig.Database, mysqlConfig.Username, mysqlConfig.Password);
            for (int i = 0; i < mysqlConfig.PoolSize; i ++)
            {
                MySqlConnection mySqlConnection = new MySqlConnection(connectStr);
                connectors.Push(mySqlConnection);
            }
        }

        public void Update()
        {

        }

        public void Fini()
        {

        }
    }
}
