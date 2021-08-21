using Cacheapi;
using Frame;
using Google.Protobuf;
using ProxyServerApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CacheServerApi
{
    public class CacheServerModule: Singleton<CacheServerModule>
    {
        ProxyModule proxyModule;
        ulong SyncIter = 0;
        DispatcherLite<Cacheapi.EOpCode, Cacheapi.Head> dispatcher = new DispatcherLite<Cacheapi.EOpCode, Cacheapi.Head>(head => head.Msgid);
        Dictionary<ulong, object> tcss = new Dictionary<ulong, object>();
        public void Init(ProxyModule proxyModule)
        {
            this.proxyModule = proxyModule;
            proxyModule.Bind("CacheServer", DataHandler);
            dispatcher.Bind<QueryRsp>(EOpCode.QueryRsp, Callback);
            dispatcher.Bind<SaveRsp>(EOpCode.SaveRsp, Callback);
            dispatcher.Bind<DeleteRsp>(EOpCode.DeleteRsp, Callback);
        }

        public Task<(Cacheapi.Head, Cacheapi.QueryRsp)> QueryAsync(string table, string key)
        {
            TaskCompletionSource<(Cacheapi.Head, Cacheapi.QueryRsp)> tcs = new TaskCompletionSource<(Cacheapi.Head, Cacheapi.QueryRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, tcs);
            Cacheapi.Head head = new Cacheapi.Head() { Msgid = Cacheapi.EOpCode.QueryReq, Sync = sync };
            Cacheapi.QueryReq queryReq = new Cacheapi.QueryReq() { Key = key, Table = table };
            var data = ProtoUtil.Pack(head, queryReq);
            _ = proxyModule.SendToAsync(new Proxyapi.TargetSvr { Id = 1, Name = "CacheServer", Type = Proxyapi.ETransmitType.Poll, Zone = 0 }, Proxyapi.EPackType.Request, data);
            Timer.Instance.SetTimeOut(5000, () => {
                var obj = tcss.GetValueOrDefault(head.Sync);
                if (obj != null)
                {
                    var tcs = (TaskCompletionSource<(Cacheapi.Head, QueryRsp)>)obj;
                    tcss.Remove(head.Sync); head.Msgid = EOpCode.QueryRsp; head.Errcode = EErrno.TimeOut;
                    tcs.SetResult((head, new QueryRsp { }));
                }
            });
            return tcs.Task;
        }
        public Task<(Cacheapi.Head, Cacheapi.SaveRsp)> SaveAsync(string table, string key, RecordInfo recordInfo)
        {
            TaskCompletionSource<(Cacheapi.Head, Cacheapi.SaveRsp)> tcs = new TaskCompletionSource<(Cacheapi.Head, Cacheapi.SaveRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, tcs);
            Head head = new Head() { Msgid = EOpCode.SaveReq, Sync = sync };
            SaveReq saveReq = new SaveReq() { Key = key, Table = table, Record = recordInfo };
            var data = ProtoUtil.Pack(head, saveReq);
            _ = proxyModule.SendToAsync(new Proxyapi.TargetSvr { Id = 1, Name = "CacheServer", Type = Proxyapi.ETransmitType.Poll, Zone = 0 }, Proxyapi.EPackType.Request, data);
            Timer.Instance.SetTimeOut(5000, () => {
                var obj = tcss.GetValueOrDefault(head.Sync);
                if (obj != null)
                {
                    var tcs = (TaskCompletionSource<(Cacheapi.Head, SaveRsp)>)obj;

                    tcss.Remove(head.Sync); head.Msgid = EOpCode.SaveRsp; head.Errcode = EErrno.TimeOut;
                    tcs.SetResult((head, new SaveRsp { }));
                }
            });
            return tcs.Task;
        }

        public Task<(Cacheapi.Head, Cacheapi.DeleteRsp)> DeleteAsync(string table, string key)
        {
            TaskCompletionSource<(Cacheapi.Head, Cacheapi.DeleteRsp)> tcs = new TaskCompletionSource<(Cacheapi.Head, Cacheapi.DeleteRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, tcs);
            Head head = new Head() { Msgid = EOpCode.SaveReq, Sync = sync };
            DeleteReq deleteReq = new DeleteReq() { Key = key, Table = table};
            var data = ProtoUtil.Pack(head, deleteReq);
            _ = proxyModule.SendToAsync(new Proxyapi.TargetSvr { Id = 1, Name = "CacheServer", Type = Proxyapi.ETransmitType.Poll, Zone = 0 }, Proxyapi.EPackType.Request, data);
            Timer.Instance.SetTimeOut(5000, () => {
                var obj = tcss.GetValueOrDefault(head.Sync);
                if (obj != null)
                {
                    var tcs = (TaskCompletionSource<(Cacheapi.Head, DeleteRsp)>)obj;

                    tcss.Remove(head.Sync); head.Msgid = EOpCode.DeleteRsp; head.Errcode = EErrno.TimeOut;
                    tcs.SetResult((head, new DeleteRsp { }));
                }
            });
            return tcs.Task;
        }




        private async Task Callback<TRsp>(Head head, TRsp rsp) where TRsp: IMessage
        {
            var obj = tcss.GetValueOrDefault(head.Sync);
            if (obj != null)
            {
                var tcs = (TaskCompletionSource<(Cacheapi.Head, TRsp)>)obj;
                tcss.Remove(head.Sync);
                await Task.Run(() => {
                    var ret = tcs.TrySetResult((head, rsp));
                });
                await Task.Delay(1);
            }
        }
        private void DataHandler(byte[] data)
        {
            dispatcher.DispatcherRequest(data);
        }

        public void Update()
        {

        }

        public void Fini()
        {

        }
    }
}
