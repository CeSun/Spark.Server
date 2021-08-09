using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer
{
    public class ProxyConfig : BaseNetConfig
    {

    }
    public class Server : ServerBaseWithNet<Server, ProxyConfig>
    {
        protected override string ConfPath => "../ProxyServerConfig.xml";

        protected override void OnHandlerConnected(Session session)
        {

        }

        protected override void OnHandlerData(Session session, byte[] data)
        {

        }

        protected override void OnHandlerDisconnected(Session session)
        {

        }
    }
}
