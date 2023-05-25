using Frame;
using Frame.NetDrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{

    class GameServerApplication : Application
    {
        protected override void OnClientConnected(Session session)
        {
        }

        protected override void OnClientDisconnected(Session session)
        {
        }

        protected override void OnClientReceiveData(Session session, Span<byte> data)
        {
        }
    }
}
