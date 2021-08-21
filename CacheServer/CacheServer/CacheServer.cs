using Cacheapi;
using CacheServer.Modules;
using CacheServer.Tables;
using CacheServerApi;
using Frame;
using Google.Protobuf;
using ProxyServerApi;
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
        Dispatcher<Cacheapi.EOpCode, Cacheapi.Head, ISession> dispatcher = new Dispatcher<Cacheapi.EOpCode, Cacheapi.Head, ISession>(head => head.Msgid);
        protected override void OnInit()
        {
            tables["DBUin"] = new TBUin();
            tables["DBAccount"] = new TBAccount();
            tables["DBNickname"] = new TBNickname();
            tables["DBPlayer"] = new TBNickname();
            Redis.Instance.Init(Config.Redis);
            Mysql.Instance.Init(Config.Mysql);
            if (Config.CacheServer.SaveInterval <= 0)
            {
                Config.CacheServer.SaveInterval = 1000 * 15 * 1;
            }
            Timer.Instance.SetInterval(Config.CacheServer.SaveInterval, () => { _ = SaveDbAsync(); });
            ProxyModule.Instance.Init(Config.IpAndPoint , new ServerInfo { id = 1, name = "CacheServer", zone = 0 });
            ProxyModule.Instance.DataHandler = DataHandler;
            dispatcher.Bind<QueryReq>(EOpCode.QueryReq, HandlerQuery);
            dispatcher.Bind<SaveReq>(EOpCode.SaveReq, HandlerSave);
            dispatcher.Bind<DeleteReq>(EOpCode.DeleteReq, HandlerDelete);
        }

        public async Task HandlerQuery(ISession session, Head reqHead, QueryReq reqBody)
        {
            Head rspHead = new Head() { Msgid = EOpCode.QueryRsp, Errcode = EErrno.Succ, Sync = reqHead.Sync};
            QueryRsp rspBody = new QueryRsp();
            var table = tables.GetValueOrDefault(reqBody.Table);
            if (table == null)
            {
                rspHead.Errcode = EErrno.TableIsNotExisted;
            }
            else
            {
                try
                {

                    var infoAndErr = await table.QueryAsync(reqBody.Key);
                    if (infoAndErr.Item2 != EErrno.Succ)
                    {
                        rspHead.Errcode = infoAndErr.Item2;
                    }
                    else
                    {
                        rspBody.Record = infoAndErr.Item1;
                    }
                } catch (Exception ex)
                {

                }
                
            }
            var data = ProtoUtil.Pack(rspHead, rspBody);
            await session.SendAsync(data);
        }

        public async Task HandlerSave(ISession session, Head reqHead, SaveReq reqBody)
        {
            Head rspHead = new Head() { Msgid = EOpCode.SaveRsp, Errcode = EErrno.Succ, Sync = reqHead.Sync};
            SaveRsp rspBody = new SaveRsp();
            var table = tables.GetValueOrDefault(reqBody.Table);
            if (table == null)
            {
                rspHead.Errcode = EErrno.TableIsNotExisted;
            }
            else
            {
                if (reqBody.Record.Version == 0)
                {
                    var err = await table.InsertAsync(reqBody.Key,reqBody.Record);
                    rspHead.Errcode = err;
                } else
                {
                    var err = await table.UpdateAsync(reqBody.Key, reqBody.Record);
                    rspHead.Errcode = err;
                }
            }
            var data = ProtoUtil.Pack(rspHead, rspBody);
            await session.SendAsync(data);
        }

        public async Task HandlerDelete(ISession session, Head reqHead, DeleteReq reqBody)
        {
            Head rspHead = new Head() { Msgid = EOpCode.DeleteRsp, Errcode = EErrno.Succ, Sync = reqHead.Sync };
            DeleteRsp rspBody = new DeleteRsp();
            var table = tables.GetValueOrDefault(reqBody.Table);
            if (table == null)
            {
                rspHead.Errcode = EErrno.TableIsNotExisted;
            }
            else
            {
                var error = await table.DeleteAsync(reqBody.Key);
                rspHead.Errcode = error;
            }
            var data = ProtoUtil.Pack(rspHead, rspBody);
            await session.SendAsync(data);
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
        protected void DataHandler(ISession session, byte[] data)
        {
            dispatcher.DispatcherRequest(session, data);
        }

        protected override void OnUpdate()
        {
            ProxyModule.Instance.Update();
            Mysql.Instance.Update();
        }
        protected override void OnFini()
        {
            Redis.Instance.Fini();
            Mysql.Instance.Fini();
        }

    }
}
