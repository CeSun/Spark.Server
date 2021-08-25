using Frame;
using Google.Protobuf;
using Proxyapi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirServerApi;
using System.Xml.Serialization;

namespace ProxyServer
{
    public class Config : BaseNetConfig
    {
        [XmlArray("IpAndPoint"), XmlArrayItem("value")]
        public string[] IpAndPoint;
    }
    public class Server : ServerBaseWithNet<Server, Config>
    {
        protected override string ConfPath => "../ProxyServerConfig.xml";

        Dispatcher<EOpCode, SHead, Session, EErrno> dispatcher = new Dispatcher<EOpCode, SHead, Session, EErrno>(new Dispatcher<EOpCode, SHead, Session, EErrno>.Config
        {
            FunGetMsgId = head => head.Msgid,
            FunInitHead = (ref SHead rspHead, SHead ReqHead, EOpCode msgId, EErrno err) =>
            {
                rspHead.Errcode = err;
                rspHead.Msgid = msgId;
                rspHead.Sync = ReqHead.Sync;
            },
            ExceptionErrCode = EErrno.Fail
        });

        DirServerModule dirServerModule = new DirServerModule();
        protected override void OnInit()
        {
            base.OnInit();
            dispatcher.Bind<HeartBeatReq, HeartBeatRsp>(EOpCode.HeartbeatReq, EOpCode.HeartbeatRsp, HeartBeatReqAsync);
            dispatcher.Bind<RegistReq, RegistRsp>(EOpCode.RegisteReq, EOpCode.RegisteRsp, RegistReqAsync);
            dispatcher.Bind<RegistReq, RegistRsp>(EOpCode.Transmit, EOpCode.Transmit, async (session, head, body) => default);
            dispatcher.Filter = Filter;
            dirServerModule.Init(Config.IpAndPoint);
            CoroutineUtil.Instance.New(Regist);
        }

        async Task Regist()
        {
           Dirapi.RegisterReq req= new Dirapi.RegisterReq { Info = new Dirapi.ServerInfo { Id = 1, Name = "ProxyServer", Zone = 0, Url = new Dirapi.IpAndPort { Ip = "127.0.0.1", Port = Config.Network.Port } } };

           var rsp = await dirServerModule.Send<Dirapi.RegisterReq, Dirapi.RegisterRsp>(Dirapi.EOpCode.RegisterReq, req);

            if (rsp.Item1.Errcode != Dirapi.EErrno.Succ)
            {
                throw new Exception();
            }

        }
        async Task<(SHead, IMessage)> Filter (Session session, SHead head, TaskAction<SHead> next, int offset, byte[] data)
        {
            if (head.Msgid != EOpCode.Transmit)
            {
                return await next();
            }
            var svrs = servers.GetValueOrDefault(head.Target.Name);
            if (svrs == default)
            {
                // todo 打log
                return default;
            }
            var svrSet = svrs.GetValueOrDefault(head.Target.Zone);
            if (svrSet == default)
            {
                // todo 打log
                return default;
            }

            var svrInfo = session.GetProcess<RegistReq>();
            if (svrInfo == null)
                return default;

            var body = data.Skip(offset).ToArray();
            SHead toHead = new SHead { Msgid = EOpCode.Transmit, Target = new TargetSvr { Id = svrInfo.Id, Name = svrInfo.Name, Zone = svrInfo.Zone }, Type = head.Type, Sync = head.Sync};
            var headBits = toHead.ToByteArray();
            var packLength = headBits.Length + body.Length + 2 * sizeof(int);
            var packLengthBits = BitConverter.GetBytes(packLength);
            var headLengthBits = BitConverter.GetBytes(headBits.Length);
            Array.Reverse(packLengthBits);
            Array.Reverse(headLengthBits);
            var toData = new byte[packLength];
            packLengthBits.CopyTo(toData, 0);
            headLengthBits.CopyTo(toData, sizeof(int));
            headBits.CopyTo(toData, 2 * sizeof(int));
            body.CopyTo(toData, 2 * sizeof(int) + headBits.Length);
            try
            {
                await svrSet.SendTo(head.Target, toData);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return default;

        }
        async Task<(SHead, RegistRsp)> RegistReqAsync (Session session, SHead head, RegistReq reqBody)
        {
            var serverTypeSet = servers.GetValueOrDefault(reqBody.Name);
            if (serverTypeSet == null)
            {
                serverTypeSet = new Dictionary<int, ServerSet>();
                servers.Add(reqBody.Name, serverTypeSet);
            }
            var ServerSet = serverTypeSet.GetValueOrDefault(reqBody.Zone);
            if (ServerSet == null)
            {
                ServerSet = new ServerSet();
                serverTypeSet.Add(reqBody.Zone, ServerSet);
            }
            var ret = ServerSet.InsertServer(session, reqBody.Id);
            if (ret)
                session.SetProcess(reqBody);
            SHead rspHead = new SHead{Msgid = EOpCode.RegisteRsp, Sync = head.Sync, Errcode = ret?EErrno.Succ:EErrno.Duplicate, Type = EPackType.Response};
            RegistRsp rspBody = new RegistRsp();
            return (rspHead, rspBody);
        }

        async Task<(SHead, HeartBeatRsp)> HeartBeatReqAsync(Session session, SHead head, HeartBeatReq reqBody)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.HeartbeatRsp, Sync = head.Sync,Errcode = EErrno.Succ, Type = EPackType.Response };
            HeartBeatRsp rspBody = new HeartBeatRsp();
            return (rspHead,  rspBody);
        }
        protected override void OnHandlerConnected(Session session)
        {

        }

        protected override async Task OnHandlerData(Session session, byte[] data)
        {
            var rsp = await dispatcher.DispatcherRequest(session, data);
            if (rsp != default)
             await session.SendAsync(ProtoUtil.Pack(rsp.head, rsp.body));
        }

        protected override void OnHandlerDisconnected(Session session)
        {
            var svrInfo = session.GetProcess<RegistReq>();
            if (svrInfo != null)
            {
                var typeSet = servers.GetValueOrDefault(svrInfo.Name);
                if (typeSet != null)
                {
                    var svrSet = typeSet.GetValueOrDefault(svrInfo.Zone);
                    svrSet.RemoveServer(svrInfo.Id);
                }
            }
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            dirServerModule?.Update();
        }
        protected override void OnFini()
        {
            base.OnFini();
            dirServerModule?.Fini();
        }

        Dictionary<string, Dictionary<int, ServerSet>> servers = new Dictionary<string, Dictionary<int, ServerSet>>();
    }
}
