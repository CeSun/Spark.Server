using Dirapi;
using Frame;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace DirServerApi
{
    public class DirServerModule
    {
        List<TcpClient> sockets = new List<TcpClient>();
        DispatcherLite<EOpCode, SHead, EErrno> dispatcher = new DispatcherLite<EOpCode, SHead, EErrno>(
         new DispatcherLite<EOpCode, SHead, EErrno>.Config
         {
             FunGetMsgId = head => head.Msgid
         });
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
            dispatcher.Bind<RegisterRsp, RegisterRsp>(EOpCode.RegisterRsp, EOpCode.RegisterRsp, Handler);
            dispatcher.Bind<SyncRsp, SyncRsp>(EOpCode.SyncRsp, EOpCode.SyncRsp, Handler);
            dispatcher.Bind<GetRsp, GetRsp>(EOpCode.GetRsp, EOpCode.GetRsp, Handler);
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
        
        private async Task<(SHead, TRsp)> Handler<TRsp>(SHead head, TRsp rsp) where TRsp : IMessage
        {
            var obj = tcss.GetValueOrDefault(head.Sync);
            if (obj == null)
                return default;
            tcss.Remove(head.Sync);
            var tcs = obj as TaskCompletionSource<(SHead, TRsp)>;
            tcs.SetResult((head, rsp));
            return default;
        }



        private async Task Revice(TcpClient client)
        {
            var buffer = new byte[1024 * 1024];
            var StartIndex = 0;
            var stream = client.GetStream();
            int len = 0;
            int nextReadLen = 4;
            bool isReadHead = true;
            while ((len = await stream.ReadAsync(buffer, StartIndex, nextReadLen)) != 0)
            {
                if (isReadHead)
                {
                    var PackLenBits = buffer.Take(4).ToArray();
                    Array.Reverse(PackLenBits);
                    nextReadLen = BitConverter.ToInt32(PackLenBits, 0) - 4;
                    StartIndex = 4;
                    isReadHead = false;
                }
                else
                {
                    var data = buffer.Take(nextReadLen + 4).ToArray();
                    dataHandler(data);
                    nextReadLen = 4;
                    StartIndex = 0;
                    isReadHead = true;
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
