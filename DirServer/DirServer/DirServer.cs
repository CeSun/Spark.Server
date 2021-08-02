using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dirapi;

namespace DirServer
{
    class Server : ServerApp<Server>
    {
        Dispatcher<EOpCode, SHead> dispatcher = new Dispatcher<EOpCode, SHead>(head => { return head.Msgid; });
        protected override void OnFini()
        {
            dispatcher.Bind<RegisterReq>(EOpCode.RegisterReq, RegisterServerHandler);
        }

        protected override async Task OnHandlerData(Session session, byte[] data)
        {
            await dispatcher.DispatcherRequest(session, data);
        }

        async Task RegisterServerHandler(Session session, SHead reqHead, RegisterReq reqBody)
        {
            await session.SendAsync(null);
        }
        protected override void OnInit()
        {

        }

        protected override void OnUpdate()
        {

        }
    }
}
