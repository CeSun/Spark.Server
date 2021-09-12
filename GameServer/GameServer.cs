using Frame;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer
{
    [Dispatcher]
    public partial class GameServer : ServerWithNet
    {
        
        protected override async Task ConnectHandler(ISession session)
        {

        }

        
        [Controller(1)]
        protected override async Task DataHandler(ISession session, byte[] data)
        {
           await Dispatcher(session, data);
        }
        
        protected override async Task DisconnectHandler(ISession session)
        {

        }
        [DispatchMethod]
        private partial Task Dispatcher(ISession session, byte[] data);
    }

}

