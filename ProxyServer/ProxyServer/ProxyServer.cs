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

        Dispatcher<EOpCode, SHead, Session> dispatcher = new Dispatcher<EOpCode, SHead, Session>(head => head.Msgid);

        DirServerModule dirServerModule = new DirServerModule();
        protected override void OnInit()
        {
            base.OnInit();
            dispatcher.Bind<HeartBeatReq>(EOpCode.HeartbeatReq, HeartBeatReqAsync);
            dispatcher.Bind<RegistReq>(EOpCode.RegisteReq, RegistReqAsync);
            dispatcher.Filter = Filter;
            dirServerModule.Init(Config.IpAndPoint);
            _ = Regist();
        }

        async Task Regist()
        {
            Dirapi.RegisterReq req= new Dirapi.RegisterReq { Info = new Dirapi.ServerInfo { Id = 1, Name = "ProxyServer", Zone = 0, Url = new Dirapi.IpAndPort { Ip = Config.Network.Host, Port = Config.Network.Port } } };

           var rsp = await dirServerModule.Send<Dirapi.RegisterReq, Dirapi.RegisterRsp>(Dirapi.EOpCode.RegisterReq, req);

        }
        async Task Filter (Session session, SHead head, TaskAction next, int offset, byte[] data)
        {
            if (head.Msgid != EOpCode.Transmit)
            {
                await next();
            }
            var svrs = servers.GetValueOrDefault(head.Target.Name);
            if (svrs == default)
            {
                // todo 打log
                return;
            }
            var svrSet = svrs.GetValueOrDefault(head.Target.Zone);
            if (svrSet == default)
            {
                // todo 打log
                return;
            }

            var svrInfo = session.GetProcess<RegistReq>();
            if (svrInfo == null)
                return;

            var body = data.Skip(offset).ToArray();
            SHead toHead = new SHead { Msgid = EOpCode.Transmit, Target = new TargetSvr { Id = svrInfo.Id, Name = svrInfo.Name, Zone = svrInfo.Zone } };
            var headBits = toHead.ToByteArray();
            var packLength = body.Length + body.Length + 2 * sizeof(int);

            var packLengthBits = BitConverter.GetBytes(packLength);
            var headLengthBits = BitConverter.GetBytes(headBits.Length);
            Array.Reverse(packLengthBits);
            Array.Reverse(headLengthBits);
            var toData = new byte[packLength];
            packLengthBits.CopyTo(toData, 0);
            headLengthBits.CopyTo(toData, sizeof(int));
            headBits.CopyTo(toData, 2 * sizeof(int));
            body.CopyTo(toData, 2 * sizeof(int) + headBits.Length);
            await svrSet.SendTo(head.Target, toData);

        }
        async Task RegistReqAsync (Session session, SHead head, RegistReq reqBody)
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
            SHead rspHead = new SHead{Msgid = EOpCode.RegisteRsp, Sync = head.Sync, Errcode = ret?EErrno.Succ:EErrno.Duplicate};
            RegistRsp rspBody = new RegistRsp();
            var data = ProtoUtil.Pack(rspHead, rspBody);
            await session.SendAsync(data);
        }

        async Task HeartBeatReqAsync(Session session, SHead head, HeartBeatReq reqBody)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.HeartbeatRsp, Sync = head.Sync,Errcode = EErrno.Succ};
            HeartBeatRsp rspBody = new HeartBeatRsp();
            var data = ProtoUtil.Pack(rspHead, rspBody);
            await session.SendAsync(data);
        }
        protected override void OnHandlerConnected(Session session)
        {

        }

        protected override void OnHandlerData(Session session, byte[] data)
        {
            dispatcher.DispatcherRequest(session, data);
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
