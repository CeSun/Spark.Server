using Frame;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServerApi
{
    public struct ProxyConfig 
    {
         
    }

    public class ProxySession : ISession
    {

        internal Proxyapi.TargetSvr Target;
        public async Task SendAsync(byte[] data)
        {
            await ProxyModule.Instance.SendToAsync(Target, data);
        }
    }
    public class ProxyModule : Singleton<ProxyModule>
    {
        TcpClient tcpClient;
        NetworkStream stream;
        Dictionary<string, HandleData> funs = new Dictionary<string, HandleData>();
        Dictionary<Proxyapi.TargetSvr, ProxySession> sessions = new Dictionary<Proxyapi.TargetSvr, ProxySession>();
        public void Init()
        {
            _ = RunAsync();
            // 心跳
            Timer.Instance.SetInterval(1000 * 30, () => {
                if (stream != null)
                {
                    Proxyapi.SHead head = new Proxyapi.SHead { Msgid = Proxyapi.EOpCode.HeartbeatReq };
                    Proxyapi.HeartBeatReq req = new Proxyapi.HeartBeatReq();
                    var headBits = head.ToByteArray();
                    var bodyBits = req.ToByteArray();
                    var packlenbits = BitConverter.GetBytes(headBits.Length + bodyBits.Length + 2 * sizeof(int));
                    var headlenbits = BitConverter.GetBytes(headBits.Length);
                    byte[] SendBuffer = new byte[headBits.Length + bodyBits.Length + 2 * sizeof(int)];
                    packlenbits.CopyTo(SendBuffer, 0);
                    headlenbits.CopyTo(SendBuffer, sizeof(int));
                    headBits.CopyTo(SendBuffer, 2 * sizeof(int));
                    bodyBits.CopyTo(SendBuffer, 2 * sizeof(int) + headBits.Length);
                    _ = stream.WriteAsync(SendBuffer);
                }
            });
        }

        public delegate void HandleData(ISession session, byte[] data);
        public void Bind(string svrName, HandleData func)
        {
            funs[svrName] = func;
        }

        public async Task SendToAsync(Proxyapi.TargetSvr targetSvr, byte[] data)
        {
            if (stream != null)
            {
                Proxyapi.SHead head = new Proxyapi.SHead {Msgid = Proxyapi.EOpCode.Transmit, Target = targetSvr };
                var headBits = head.ToByteArray();
                var packlenbits = BitConverter.GetBytes(headBits.Length + data.Length + 2 * sizeof(int));
                var headlenbits = BitConverter.GetBytes(headBits.Length);
                byte[] SendBuffer = new byte[headBits.Length + data.Length + 2 * sizeof(int)];
                packlenbits.CopyTo(SendBuffer, 0);
                headlenbits.CopyTo(SendBuffer, sizeof(int));
                headBits.CopyTo(SendBuffer, 2 * sizeof(int));
                data.CopyTo(SendBuffer, 2 * sizeof(int) + headBits.Length);
                await stream.WriteAsync(SendBuffer);
            }
        }
        private async Task RunAsync()
        {
            var buffer = new byte[1024 * 1024 * 2];
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 2009);
            stream = tcpClient.GetStream();
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
                        dispatcher(data);
                        data = buffer.Skip(PackLen).Take(StartIndex - PackLen).ToArray();
                        Array.Copy(data, buffer, data.Length);
                        StartIndex = StartIndex - PackLen;
                    }
                }
            }
        }

        private void dispatcher (byte[] data)
        {
            var packlenbits = data.Take(4).ToArray();
            Array.Reverse(packlenbits);
            var packlen = BitConverter.ToInt32(packlenbits, 0);
            var headlenbits = data.Skip(4).Take(4).ToArray();
            Array.Reverse(headlenbits);
            var headlen = BitConverter.ToInt32(headlenbits, 0);
            var head = Proxyapi.SHead.Parser.ParseFrom(data, 2 * sizeof(int), headlen);
            var session = sessions.GetValueOrDefault(head.Target);
            if (session == null)
            {
                session = new ProxySession { Target = head.Target };
                sessions[head.Target] = session;
            }
            var fun = funs.GetValueOrDefault(head.Target.Name);
            if (fun != null)
                fun(session, data.Skip(2 * sizeof(int) + headlen).ToArray());
        }
    }
}
