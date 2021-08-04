using DataBase;
using DataBase.Tables;
using Frame;
using GameServer.Module;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest
{
    class Program
    {
        static UinMngr uinMngr = new UinMngr();
        static void Main(string[] args)
        {
            SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            Database.Init(null);
            uinMngr.Init(1);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            TestUinMngr();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            while (true)
            {
                try
                {
                    SyncContext.Update();
                    uinMngr.Update();
                } catch
                { break; }
            }
            uinMngr.Fini();
            Database.Fini();

        }
        static async Task TestDataBase()
        {
            List<Task<(TPlayer, DBError)>> players = new List<Task<(TPlayer, DBError)>>();
            for(var i = 0; i < 2; i ++)
            {
                players.Add(TPlayer.QueryAync((1, 1)));
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

            var ret = await players2[0].SaveAync();
            Console.WriteLine(ret);
            ret = await players2[1].SaveAync();
            Console.WriteLine(ret);

        }

        static async Task TestUinMngr()
        {
            for (var i = 0; i < 1000; i++)
            {
                var uin = await uinMngr.GetUinAsync();
                Console.WriteLine("uin:" + uin);
            }
        }
    }
}
