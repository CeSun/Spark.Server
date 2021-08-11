using CacheServer.Modules;
using CacheServer.Tables;
using Frame;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheServer
{
    
    class CacheServer : ServerBase<CacheServer, Config>
    {
        protected override string ConfPath => "../CacheServerConfig.xml";
        protected Dictionary<string, Table> tables = new Dictionary<string, Table>();
        protected override void OnInit()
        {
            tables["DBUin"] = new TBUin();
            tables["DBAccount"] = new TBAccount();
            tables["DBNickname"] = new TBNickname();
            tables["DBPlayer"] = new TBNickname();
            Redis.Instance.Init(Config.Redis);
            Mysql.Instance.Init(Config.Mysql);
            Timer.Instance.Init();
            if (Config.CacheServer.SaveInterval <= 0)
            {
                Config.CacheServer.SaveInterval = 1000 * 60 * 3;
            }
            Timer.Instance.SetInterval(Config.CacheServer.SaveInterval, () => { _ = SaveDbAsync(); });
        }
        
        /// <summary>
        /// 保存脏数据
        /// </summary>
        /// <returns></returns>
        public async Task SaveDbAsync()
        {
            var redis = Redis.Instance.Database;
            var token = "123";
            if (await redis.LockTakeAsync("save_local", token, TimeSpan.FromMinutes(10)))
            {
                try
                {
                    await redis.SetPopAsync("");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    await redis.LockReleaseAsync("save_local", token);
                }
            }


        }
        protected void DataHandler( byte[] data)
        {
        }

        protected override void OnUpdate()
        {
            // Redis.Instance.Update();
            //  Mysql.Instance.Update();
            Timer.Instance.Update();
        }
        protected override void OnFini()
        {
            Redis.Instance.Fini();
            Mysql.Instance.Update();
        }

    }
}
