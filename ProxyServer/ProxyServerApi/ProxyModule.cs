using DirServerApi;
using Frame;
using Google.Protobuf;
using Proxyapi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ProxyServerApi
{
    
    public class ProxySession : ISession
    {
        internal ulong sync = 0;
        internal Proxyapi.TargetSvr Target;
        public async Task SendAsync(byte[] data)
        {
            await ProxyModule.Instance.SendToAsync(Target, Proxyapi.EPackType.Response,data, 0);
        }
    }

    public struct ServerInfo {
        public int id;
        public string name;
        public int zone;
    }

    public class ProxyModule : Singleton<ProxyModule>
    {
        ulong SyncIter = 0;
        Dictionary<string, ClientHandlerData> funs = new Dictionary<string, ClientHandlerData>();
        List<TcpClient> tcpClients = new List<TcpClient>();
        DirServerModule dirServerModule = new DirServerModule();
        public HandleData DataHandler { get; set; }
        public void Init(string[] dirlist, ServerInfo serverInfo)
        {
           
            dirServerModule.Init(dirlist);
            CoroutineUtil.Instance.New(async () => await GetProxyListAsync(serverInfo));
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
                        CoroutineUtil.Instance.New(async () => await stream.WriteAsync(data));
                        // 心跳
                        Frame.Timer.Instance.SetInterval(1000 * 30, () => {
                        TaskAction fun = async () =>
                        {
                            if (stream != null)
                            {
                                Proxyapi.SHead head = new Proxyapi.SHead { Msgid = Proxyapi.EOpCode.HeartbeatReq };
                                Proxyapi.HeartBeatReq req = new Proxyapi.HeartBeatReq();
                                var headBits = head.ToByteArray();
                                var bodyBits = req.ToByteArray();
                                var packlenbits = BitConverter.GetBytes(headBits.Length + bodyBits.Length + 2 * sizeof(int));
                                Array.Reverse(packlenbits);
                                var headlenbits = BitConverter.GetBytes(headBits.Length);
                                Array.Reverse(headlenbits);
                                byte[] SendBuffer = new byte[headBits.Length + bodyBits.Length + 2 * sizeof(int)];
                                packlenbits.CopyTo(SendBuffer, 0);
                                headlenbits.CopyTo(SendBuffer, sizeof(int));
                                headBits.CopyTo(SendBuffer, 2 * sizeof(int));
                                bodyBits.CopyTo(SendBuffer, 2 * sizeof(int) + headBits.Length);
                                await stream.WriteAsync(SendBuffer);
                            }
                        };
                         CoroutineUtil.Instance.New(fun);
                    });
                        CoroutineUtil.Instance.New(async () => await RecvieAsync(tcpClient));

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

        public async Task SendToAsync(Proxyapi.TargetSvr targetSvr,Proxyapi.EPackType type, byte[] data, ulong sync)
        {
            // var sync = ++SyncIter;
            var client = tcpClients.FirstOrDefault();
            if (client == null)
            {
                return;
            }
            var stream = client.GetStream();
            Proxyapi.SHead head = new Proxyapi.SHead {Msgid = Proxyapi.EOpCode.Transmit, Target = targetSvr , Type = type, Sync = sync};
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
            var buffer = new byte[1024 * 1024];
            var stream = tcpClient.GetStream();
            var start = 0;
            int len = 0;
            int packLen = 0;
            while ((len = await stream.ReadAsync(buffer, start, buffer.Length - start)) != 0)
            {
                var bitsCount = start + len;
                if (bitsCount > 4)
                {
                    if (packLen == 0)
                    {
                        var packLenBits = buffer.Take(4).ToArray();
                        Array.Reverse(packLenBits);
                        packLen = BitConverter.ToInt32(packLenBits);
                    }
                    var offset = 0;
                    var count = bitsCount;
                    while (count >= packLen)
                    {
                        var data = buffer.Skip(offset).Take(packLen).ToArray();
                        dispatcher( data);
                        offset += packLen;
                        count -= packLen;
                        packLen = 0;
                        if (count >= 4)
                        {
                            var packLenBits = buffer.Skip(offset).Take(4).ToArray();
                            Array.Reverse(packLenBits);
                            packLen = BitConverter.ToInt32(packLenBits);
                        }
                        else
                        {
                            break;
                        }
                    }
                    var halfpack = buffer.Skip(offset).Take(bitsCount - offset).ToArray();
                    halfpack.CopyTo(buffer, 0);
                    start = halfpack.Length;
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
                head.Target.Type = Proxyapi.ETransmitType.Direction;
                var session = new ProxySession { Target = head.Target, sync = head.Sync };
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
