using Frame;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer
{

    public class GameServer : ServerWithNet
    {
        enum ET
        {
            ABC
        }
        protected override async Task ConnectHandler(ISession session)
        {
        }
        
        protected override async Task DataHandler(ISession session, byte[] data)
        {

        }

        protected override async Task DisconnectHandler(ISession session)
        {

        }
    }
}
