﻿using Cacheapi;
using Frame;
using Google.Protobuf;
using ProxyServerApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheServerApi
{
    public class CacheServerModule: Singleton<CacheServerModule>
    {
        ProxyModule proxyModule;
        ulong SyncIter = 0;
        DispatcherLite<EOpCode, Head> dispatcher = new DispatcherLite<EOpCode, Head>(head => head.Msgid);
        Dictionary<ulong, (object, Stopwatch )> tcss = new Dictionary<ulong, (object, Stopwatch)>();
        public void Init(ProxyModule proxyModule)
        {
            this.proxyModule = proxyModule;
            proxyModule.Bind("CacheServer", DataHandler);
            dispatcher.Bind<QueryRsp>(EOpCode.QueryRsp, Callback);
            dispatcher.Bind<SaveRsp>(EOpCode.SaveRsp, Callback);
            dispatcher.Bind<DeleteRsp>(EOpCode.DeleteRsp, Callback);
        }
        public Task<(Head, QueryRsp)> QueryAsync(string table, string key)
        {
            TaskCompletionSource<(Head, QueryRsp)> tcs = new TaskCompletionSource<(Head, QueryRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, (tcs, Stopwatch.StartNew()));
            Head head = new Head() { Msgid = EOpCode.QueryReq, Sync = sync };
            QueryReq queryReq = new QueryReq() { Key = key, Table = table };
            var data = ProtoUtil.Pack(head, queryReq);
            CoroutineUtil.Instance.New(async () => await proxyModule.SendToAsync(new Proxyapi.TargetSvr { Id = 1, Name = "CacheServer", Type = Proxyapi.ETransmitType.Poll, Zone = 0 }, Proxyapi.EPackType.Request, data, sync));
            Frame.Timer.Instance.SetTimeOut(4000, () => {
                var obj = tcss.GetValueOrDefault(head.Sync);
                if (obj != default)
                {
                    tcss.Remove(head.Sync);
                    var tcs = (TaskCompletionSource<(Head, QueryRsp)>)obj.Item1;
                    obj.Item2.Stop();
                    Console.WriteLine("Timeout, spend time:" + obj.Item2.Elapsed.Milliseconds);

                    head.Msgid = EOpCode.QueryRsp; head.Errcode = EErrno.TimeOut;
                    tcs.SetResult((head, new QueryRsp { }));
                }
            });
            return tcs.Task;
        }
        public Task<(Head, SaveRsp)> SaveAsync(string table, string key, RecordInfo recordInfo)
        {
            TaskCompletionSource<(Head, SaveRsp)> tcs = new TaskCompletionSource<(Head, SaveRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, (tcs, Stopwatch.StartNew()));
            Head head = new Head() { Msgid = EOpCode.SaveReq, Sync = sync };
            SaveReq saveReq = new SaveReq() { Key = key, Table = table, Record = recordInfo };
            var data = ProtoUtil.Pack(head, saveReq);
            CoroutineUtil.Instance.New(async () => await proxyModule.SendToAsync(new Proxyapi.TargetSvr { Id = 1, Name = "CacheServer", Type = Proxyapi.ETransmitType.Poll, Zone = 0 }, Proxyapi.EPackType.Request, data, sync));
            Frame.Timer.Instance.SetTimeOut(4000, () => {
                var obj = tcss.GetValueOrDefault(head.Sync);
                if (obj != default)
                {
                    tcss.Remove(head.Sync); head.Msgid = EOpCode.SaveRsp; head.Errcode = EErrno.TimeOut;
                    var tcs = (TaskCompletionSource<(Head, SaveRsp)>)obj.Item1;
                    obj.Item2.Stop();
                    Console.WriteLine("Timeout, spend time:" + obj.Item2.Elapsed.Milliseconds);
                    tcs.SetResult((head, new SaveRsp { }));
                }
            });
            return tcs.Task;
        }

        public Task<(Head, DeleteRsp)> DeleteAsync(string table, string key)
        {
            TaskCompletionSource<(Head, DeleteRsp)> tcs = new TaskCompletionSource<(Head, DeleteRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, (tcs, Stopwatch.StartNew()));
            Head head = new Head() { Msgid = EOpCode.SaveReq, Sync = sync };
            DeleteReq deleteReq = new DeleteReq() { Key = key, Table = table};
            var data = ProtoUtil.Pack(head, deleteReq);
            CoroutineUtil.Instance.New(async () => await proxyModule.SendToAsync(new Proxyapi.TargetSvr { Id = 1, Name = "CacheServer", Type = Proxyapi.ETransmitType.Poll, Zone = 0 }, Proxyapi.EPackType.Request, data, sync));
            Frame.Timer.Instance.SetTimeOut(4000, () => {
                var obj = tcss.GetValueOrDefault(head.Sync);
                if (obj != default)
                {
                    tcss.Remove(head.Sync); head.Msgid = EOpCode.DeleteRsp; head.Errcode = EErrno.TimeOut;
                    var tcs = (TaskCompletionSource<(Head, DeleteRsp)>)obj.Item1;
                    obj.Item2.Stop();
                    Console.WriteLine("Timeout, spend time:" + obj.Item2.Elapsed.Milliseconds);
                    tcs.SetResult((head, new DeleteRsp { }));
                }
            });
            return tcs.Task;
        }




        private async Task Callback<TRsp>(Head head, TRsp rsp) where TRsp: IMessage
        {
            var obj = tcss.GetValueOrDefault(head.Sync);
            if (obj != default)
            {
                var tcs = (TaskCompletionSource<(Head, TRsp)>)obj.Item1;
                obj.Item2.Stop();
                tcss.Remove(head.Sync);
                var ret = tcs.TrySetResult((head, rsp));
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
