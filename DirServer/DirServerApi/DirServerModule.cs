using Dirapi;
using Frame;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DirServerApi
{
    public class DirServerModule
    {
        List<TcpClient> sockets = new List<TcpClient>();
        DispatcherLite<EOpCode, SHead> dispatcher = new DispatcherLite<EOpCode, SHead>(head => head.Msgid);
        ulong SyncIter = 0;

        Dictionary<ulong, object> tcss = new Dictionary<ulong, object>();
        public void Init(string[] ips)
        {
            foreach (var cfg in ips)
            {
                var tcpClient = new TcpClient();
                tcpClient.Connect(IPEndPoint.Parse(cfg));
                CoroutineUtil.Instance.New(async () => await Revice(tcpClient));
                sockets.Add(tcpClient);
            }
            dispatcher.Bind<RegisterRsp>(EOpCode.RegisterRsp, Handler);
            dispatcher.Bind<SyncRsp>(EOpCode.SyncRsp, Handler);
            dispatcher.Bind<GetRsp>(EOpCode.GetRsp, Handler);
        }

        public Task<(SHead, TRsp)> Send<TReq, TRsp>(EOpCode code, TReq req) where TReq: IMessage where TRsp: IMessage
        {
            TaskCompletionSource<(SHead, TRsp)> tcs = new TaskCompletionSource<(SHead, TRsp)>();
            var sync = ++SyncIter;
            tcss.Add(sync, tcs);
            SHead head = new SHead { Msgid = code, Sync = sync };
            if (sockets.FirstOrDefault() == null)
                tcs.SetException(new Exception("没有dir链接"));
            else
            {
                var data = ProtoUtil.Pack(head, req);
                var stream = sockets.FirstOrDefault().GetStream();
                CoroutineUtil.Instance.New(async () => await stream.WriteAsync(data));
            }
            Frame.Timer.Instance.SetTimeOut(3000, () => {
                if (tcss.ContainsKey(sync))
                {
                    tcss.Remove(sync);
                    tcs.SetException(new Exception("超时"));
                }
            });
            return tcs.Task;
        }
        
        private async Task Handler<TRsp>(SHead head, TRsp rsp) where TRsp : IMessage
        {
            var obj = tcss.GetValueOrDefault(head.Sync);
            if (obj == null)
                return;
            tcss.Remove(head.Sync);
            var tcs = obj as TaskCompletionSource<(SHead, TRsp)>;
            tcs.SetResult((head, rsp));
        }



        private async Task Revice(TcpClient client)
        {
            var buffer = new byte[1024 * 1024];
            var stream = client.GetStream();
            var StartIndex = 0;
            int len = 0;
            while ((len = await stream.ReadAsync(buffer, StartIndex, buffer.Length - StartIndex)) != 0)
            {
                StartIndex = StartIndex + len;

                if (StartIndex >= 4)
                {
                    var PackLenBits = buffer.Take(4).ToArray();
                    Array.Reverse(PackLenBits);
                    var PackLen = BitConverter.ToInt32(PackLenBits, 0);
                    if (PackLen >= 1024 * 1024 * 2)
                    {
                        break;
                    }
                    if (StartIndex >= PackLen)
                    {
                        var data = buffer.Take(PackLen).ToArray();
                        dataHandler(data);
                        data = buffer.Skip(PackLen).Take(StartIndex - PackLen).ToArray();
                        Array.Copy(data, buffer, data.Length);
                        StartIndex = StartIndex - PackLen;
                    }
                }
            }

            sockets.Remove(client);
        }
        private void dataHandler(byte[] data)
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
