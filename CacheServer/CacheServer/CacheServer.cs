using CacheServer.Modules;
using CacheServer.Tables;
using CacheServerApi;
using Frame;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                    var hashEntrys = await redis.HashGetAllAsync("DirtyKey");
                    foreach (var entry in hashEntrys)
                    {
                        var data = await redis.HashGetAllAsync((string)entry.Value);
                        var strs = ((string)entry.Name).Split('|');
                        if (strs.Length < 2)
                            continue;
                        var msyqlkey = string.Join(',', strs.Skip(1).ToArray());
                        var table = tables.GetValueOrDefault(strs[0]);
                        if (table == null)
                            continue;
                        if (await table.SaveToMysqlAsync(msyqlkey, data) == EErrno.Succ)
                        {
                            Condition cond = null;
                            if (data == null || data.Length == 0)
                            {
                                cond = Condition.KeyNotExists((string)entry.Value);
                            }
                            else
                            {
                                uint version = 0;
                                foreach (var field in data)
                                {
                                    if (field.Name == "version")
                                    {
                                        version = (uint)field.Value;
                                    }
                                }
                                cond = Condition.HashEqual((string)entry.Value, "version", version);
                            }
                            var tx = redis.CreateTransaction();
                            tx.AddCondition(cond);
                            await tx.SetRemoveAsync("DirtyKey", entry.Value);
                            await tx.ExecuteAsync();
                        }
                       
                    }
                    hashEntrys = await redis.HashGetAllAsync("DeleteDirtyKey");
                    foreach (var entry in hashEntrys)
                    {

                    }
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
