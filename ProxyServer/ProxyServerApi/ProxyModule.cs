using DirServerApi;
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

namespace ProxyServerApi
{
    
    public class ProxySession : ISession
    {

        internal Proxyapi.TargetSvr Target;
        public async Task SendAsync(byte[] data)
        {
            await ProxyModule.Instance.SendToAsync(Target, Proxyapi.EPackType.Response,data);
        }
    }

    public struct ServerInfo {
        public int id;
        public string name;
        public int zone;
    }

    public class ProxyModule : Singleton<ProxyModule>
    {
        Dictionary<string, ClientHandlerData> funs = new Dictionary<string, ClientHandlerData>();
        List<TcpClient> tcpClients = new List<TcpClient>();
        DirServerModule dirServerModule = new DirServerModule();
        Dictionary<Proxyapi.TargetSvr, ProxySession> sessions = new Dictionary<Proxyapi.TargetSvr, ProxySession>();
        public HandleData DataHandler { get; set; }
        public void Init(string[] dirlist, ServerInfo serverInfo)
        {
           
            dirServerModule.Init(dirlist);
            _ = GetProxyListAsync(serverInfo);
        }
        public void Update()
        {
            dirServerModule.Update();
        }

        private async Task GetProxyListAsync(ServerInfo serverInfo)
        {
            await Task.Delay(1000);
            var req = new Dirapi.GetReq { Name = "ProxyServer", Zone = 0 };
            var rsp = await dirServerModule.Send<Dirapi.GetReq, Dirapi.GetRsp>(Dirapi.EOpCode.GetReq, req);
            try
            {

            if (rsp.Item1.Errcode == Dirapi.EErrno.Succ)
            {
                foreach(var svr in rsp.Item2.Servres)
                {
                        var tcpClient = new TcpClient();
                        await tcpClient.ConnectAsync(IPAddress.Parse(svr.Url.Ip), svr.Url.Port);
                        tcpClients.Add(tcpClient);
                        NetworkStream stream = tcpClient.GetStream();
                        Proxyapi.SHead head = new Proxyapi.SHead() {Msgid = Proxyapi.EOpCode.RegisteReq, Target = new Proxyapi.TargetSvr { Id = serverInfo.id, Name = serverInfo.name, Zone = serverInfo.zone , Type = Proxyapi.ETransmitType.Broadcast} };
                        Proxyapi.RegistReq registReq = new Proxyapi.RegistReq() {Id =  serverInfo.id, Name = serverInfo.name, Zone = serverInfo.zone};
                        var data = ProtoUtil.Pack(head, registReq);
                        _ = stream.WriteAsync(data);
                        // 心跳
                        Timer.Instance.SetInterval(1000 * 30, () => {
                        TaskAction fun = async () =>
                        {
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
                                await stream.WriteAsync(SendBuffer);
                            }
                        };
                        _ = fun();
                    });
                    _ = RecvieAsync(tcpClient);

                }
            }

            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
        public void Fini()
        {
            dirServerModule.Fini();
        }


        public delegate void HandleData(ISession session, byte[] data);
        public delegate void ClientHandlerData(byte[] data);
        public void Bind(string svrName, ClientHandlerData func)
        {
            funs[svrName] = func;
        }

        public async Task SendToAsync(Proxyapi.TargetSvr targetSvr,Proxyapi.EPackType type, byte[] data)
        {
            var client = tcpClients.FirstOrDefault();
            if (client == null)
            {
                return;
            }
            var stream = client.GetStream();
            Proxyapi.SHead head = new Proxyapi.SHead {Msgid = Proxyapi.EOpCode.Transmit, Target = targetSvr , Type = type};
            var headBits = head.ToByteArray();

            var packlenbits = BitConverter.GetBytes(headBits.Length + data.Length + 2 * sizeof(int));
            Array.Reverse(packlenbits);
            var headlenbits = BitConverter.GetBytes(headBits.Length);
            Array.Reverse(headlenbits);
            byte[] SendBuffer = new byte[headBits.Length + data.Length + 2 * sizeof(int)];
            packlenbits.CopyTo(SendBuffer, 0);
            headlenbits.CopyTo(SendBuffer, sizeof(int));
            headBits.CopyTo(SendBuffer, 2 * sizeof(int));
            data.CopyTo(SendBuffer, 2 * sizeof(int) + headBits.Length);
            await stream.WriteAsync(SendBuffer);


        }

        private async Task RecvieAsync(TcpClient tcpClient)
        {
            var buffer = new byte[1024 * 1024 * 2];
            var stream = tcpClient.GetStream();
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
            var bodydata = data.Skip(2 * sizeof(int) + headlen).ToArray();
            if (head.Type == Proxyapi.EPackType.Request)
            {
                var session = sessions.GetValueOrDefault(head.Target);
                if (session == null)
                {
                    session = new ProxySession { Target = head.Target };
                    sessions[head.Target] = session;
                }
                DataHandler?.Invoke(session, bodydata);

            } else
            {
                if (head.Target != null)
                {

                    var fun = funs.GetValueOrDefault(head.Target.Name);
                    fun?.Invoke(bodydata);
                }
            }
            
        }
    }
}
