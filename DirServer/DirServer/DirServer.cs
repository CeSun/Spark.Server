using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dirapi;
using Google.Protobuf;
using System.IO;
using System.Net;

namespace DirServer
{
    
    public class Config : BaseNetConfig
    {

    }
    class Server : ServerBaseWithNet<Server, Config>
    {
        Dispatcher<EOpCode, SHead, Session> dispatcher = new Dispatcher<EOpCode, SHead, Session>(head => { return head.Msgid; });

        Dictionary<string, ServerSet> servers = new Dictionary<string, ServerSet>();

        protected override string ConfPath => "../DirServerConfig.xml";

        protected override void OnFini()
        {
            base.OnFini();
        }

        protected override void OnHandlerData(Session session, byte[] data)
        {
             dispatcher.DispatcherRequest(session, data);
        }
        async Task RegisterServerHandler(Session session, SHead reqHead, RegisterReq reqBody)
        {
            var svrName = reqBody.Info.Name;
            var server = servers.GetValueOrDefault(svrName);
            if (server == null)
            {
                server = new ServerSet();
                servers.Add(svrName, server);
            }
            var ret = server.InsertServer(reqBody.Info.Zone, reqBody.Info.Id, new IPEndPoint(IPAddress.Parse( reqBody.Info.Url.Ip), reqBody.Info.Url.Port));
            SHead rspHead = new SHead { Errcode = ret?EErrno.Succ:EErrno.Fail, Msgid = EOpCode.RegisterRsp, Sync = reqHead.Sync };

            RegisterRsp rspBody = new RegisterRsp { };
            if (ret == true)
                session.SetProcess((svrName, reqBody.Info.Zone, reqBody.Info.Id));
            await Send(session, rspHead, rspBody);

        }

        async Task GetHandler(Session session, SHead reqHead, GetReq reqBody)
        {
            var svr = reqBody.Name;
            var zone = reqBody.Zone;
            var server = servers.GetValueOrDefault(svr);
            GetRsp rspBody = new GetRsp();
            if (server != null)
            {
                var svrs =  server.GetServers(zone);
                rspBody.Version = svrs.Item2;
                foreach(var item in svrs.Item1)
                {
                    rspBody.Servres.Add(new ServerInfo {Name = svr, Zone = zone, Id = item.Item1, Url = new IpAndPort {Ip= item.Item2.Address.ToString(), Port = item.Item2.Port } });
                }
            }
            SHead rspHead = new SHead { Errcode = EErrno.Succ, Msgid = EOpCode.GetRsp, Sync = reqHead.Sync };
            await Send(session, rspHead, rspBody);
        }

        async Task SyncHandler(Session session, SHead reqHead, SyncReq reqBody)
        {
            var svr = reqBody.Name;
            var zone = reqBody.Zone; 
            var server = servers.GetValueOrDefault(svr);
            SyncRsp rspBody = new SyncRsp();
            SHead rspHead = new SHead { Errcode = EErrno.NotNeedSync, Msgid = EOpCode.SyncRsp, Sync = reqHead.Sync };
            if (server != null)
            {
                var svrs = server.GetServers(zone);
                if (reqBody.Version != svrs.Item2)
                {
                    rspBody.Version = svrs.Item2;
                    foreach (var item in svrs.Item1)
                    {
                        rspBody.Servres.Add(new ServerInfo { Name = svr, Zone = zone, Id = item.Item1, Url = new IpAndPort { Ip = item.Item2.Address.ToString(), Port = item.Item2.Port } });
                    }
                    rspHead.Errcode = EErrno.Succ;
                }
            }
            await Send(session, rspHead, rspBody);
        }
        async Task Send<TRsp>(Session session, SHead head, TRsp rsp) where TRsp : IMessage
        {
            var bodyBits = rsp.ToByteArray();
            var headBits = head.ToByteArray();
            int packLength = bodyBits.Length + headBits.Length + 2 * sizeof(int);
            byte[] data = new byte[packLength];
            var packLengthBits = BitConverter.GetBytes(packLength);
            var headLengthBits = BitConverter.GetBytes(headBits.Length);
            Array.Reverse(packLengthBits);
            Array.Reverse(headLengthBits);
            packLengthBits.CopyTo(data, 0);
            headLengthBits.CopyTo(data, sizeof(int));
            headBits.CopyTo(data, 2 * sizeof(int));
            bodyBits.CopyTo(data, 2 * sizeof(int) + headBits.Length);
            await session.SendAsync(data);
        } 
        protected override void OnInit()
        {
            base.OnInit();
            dispatcher.Bind<RegisterReq>(EOpCode.RegisterReq, RegisterServerHandler);
            dispatcher.Bind<GetReq>(EOpCode.GetReq, GetHandler);
            dispatcher.Bind<SyncReq>(EOpCode.SyncReq, SyncHandler);
        }

        protected override void OnUpdate()
        {

            base.OnUpdate();
        }

        protected override void OnHandlerConnected(Session session)
        {
        }

        protected override void OnHandlerDisconnected(Session session)
        {
            var info = session.GetProcess<(string, int, int)>();
            if (info != default)
            {
                var server = servers.GetValueOrDefault(info.Item1);
                if (server == null)
                {
                    server.DeleteServer(info.Item2, info.Item3);
                }
            }



        }
    }
}