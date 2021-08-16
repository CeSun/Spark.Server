using Frame;
using Proxyapi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer
{
    public class Config : BaseNetConfig
    {

    }
    public class Server : ServerBaseWithNet<Server, Config>
    {
        protected override string ConfPath => "../ProxyServerConfig.xml";

        Dispatcher<EOpCode, SHead, Session> dispatcher = new Dispatcher<EOpCode, SHead, Session>(head => head.Msgid);

        protected override void OnInit()
        {
            base.OnInit();
            dispatcher.Bind<HeartBeatReq>(EOpCode.HeartbeatReq, HeartBeatReqAsync);
            dispatcher.Bind<RegistReq>(EOpCode.RegisteReq, RegistReqAsync);
            dispatcher.Filter = Filter;
        }

        async Task Filter (SHead head, TaskAction next, int offset, byte[] data)
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
            var body = data.Skip(offset).ToArray();
            await svrSet.SendTo(head.Target, body);

        }
        async Task RegistReqAsync (Session session, SHead head, RegistReq reqBody)
        {

        }

        async Task HeartBeatReqAsync(Session session, SHead head, HeartBeatReq reqBody)
        {

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

        }

        Dictionary<string, Dictionary<int, ServerSet>> servers = new Dictionary<string, Dictionary<int, ServerSet>>();
    }
}
