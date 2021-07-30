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
            TestDataBase();
            while (true)
            {
                SyncContext.Update();
            }
            Database.Fini();

        }
        static async Task TestDataBase()
        {
            List<Task<(TPlayer, DBError)>> players = new List<Task<(TPlayer, DBError)>>();
            for(var i = 0; i < 100; i ++)
            {
                players.Add(TPlayer.QueryAync(1));
            }
            List<TPlayer> players2 = new List<TPlayer>();
            foreach(var task in players)
            {
                var res = await task;
                if (res.Item2 == DBError.Success)
                {
                    players2.Add(res.Item1);
                }
            }
        }
    }
}
