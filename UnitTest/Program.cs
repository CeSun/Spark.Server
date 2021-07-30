using DataBase;
using DataBase.Tables;
using Frame;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            Database.Init();
            fun();
            while (true)
            {
                SyncContext.Update();
            }
            Database.Fini();

        }
        static async Task fun()
        {

            var p = await TPlayer.QueryAync(1);
            if (p.Error == DBError.Success)
            {
                Console.WriteLine(p.Row.Value.Nickname);
            }
            
        }
    }
}
