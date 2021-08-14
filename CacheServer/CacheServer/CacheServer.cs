using CacheServer.Modules;
using CacheServer.Tables;
using CacheServerApi;
using Frame;
using Google.Protobuf;
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
                Config.CacheServer.SaveInterval = 1000 * 15 * 1;
            }
            Timer.Instance.SetInterval(Config.CacheServer.SaveInterval, () => { _ = SaveDbAsync(); });

            _ = TestAsync();
        }

        private async Task TestAsync()
        {
            await Task.Delay(1000);
            RecordInfo recordInfo = new RecordInfo();
            recordInfo.Table = "DBNickname";
            recordInfo.Key = "ABC";
            recordInfo.Version = 1;
            var data = ByteString.CopyFrom(new byte[] { 1, 2, 3 });
            recordInfo.Field.Add(new RecordFieldInfo { Field = "base", Data = data });
            await tables["DBNickname"].InsertAsync("ABC", recordInfo);
            try
            {
                await tables["DBNickname"].UpdateAsync("ABC", recordInfo);
                recordInfo.Version++;
                await tables["DBNickname"].UpdateAsync("ABC", recordInfo);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

            }
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
                    var values = await redis.SetMembersAsync("DirtyKey");
                    foreach (var value in values)
                    {
                        var data = await redis.HashGetAllAsync((string)value);
                        var strs = ((string)value).Split('|');
                        if (strs.Length < 2)
                            continue;
                        var msyqlkey = string.Join('|', strs.Skip(1).ToArray());
                        var table = tables.GetValueOrDefault(strs[0]);
                        if (table == null)
                            continue;
                        var version = data.FirstOrDefault(res => res.Name == "version");
                        if (version == default)
                            continue;
                        if (await table.SaveToMysqlAsync(msyqlkey, data) == EErrno.Succ)
                        {
                            var lua = @"
                                if tonumber(redis.call('HGET', '{0}', 'version')) == tonumber({1}) then
                                    redis.call('SREM', 'DirtyKey', '{0}');
                                end
                                return 1
                            ";
                            lua = string.Format(lua, (string)value, (string)version.Value);
                            await redis.ScriptEvaluateAsync(lua);
                        }
                       
                    }
                    values = await redis.SetMembersAsync("DeleteDirtyKey");
                    foreach (var value in values)
                    {
                        var data = await redis.HashGetAllAsync((string)value);
                        var strs = ((string)value).Split('|');
                        if (strs.Length < 2)
                            continue;
                        var msyqlkey = string.Join('|', strs.Skip(1).ToArray());
                        var table = tables.GetValueOrDefault(strs[0]);
                        if (table == null)
                            continue;
                        var err = await table.DeleteFromMysqlAsync(msyqlkey);
                        if (err == EErrno.Succ)
                            await redis.SetRemoveAsync("DeleteDirtyKey", value);
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
                await redis.LockReleaseAsync("save_local", token);
            }
            Console.WriteLine("save success!");
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
