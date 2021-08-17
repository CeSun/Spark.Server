using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frame;
namespace ProxyServer
{
    public class ServerSet
    {
        public async Task SendTo(Proxyapi.TargetSvr target, byte[] data)
        {

            switch (target.Type )
            {
                case Proxyapi.ETransmitType.Broadcast:
                    foreach (var item in servers)
                    {
                        // 非阻塞发送 
                        _ = item.Value.SendAsync(data);
                    }
                    break;
                case Proxyapi.ETransmitType.Poll:
                    {
                        if (servers.Count == 0)
                            return;
                        if (index >= servers.Count)
                        {
                            index = 0;
                        }
                        var item = servers.Skip(index).FirstOrDefault();
                        await item.Value.SendAsync(data);
                    }
                    break;

            }
        }

        public bool InsertServer(Session session, int instanceId)
        {
            if (!servers.ContainsKey(instanceId))
            {
                servers[instanceId] = session;
            }
            else
            {
                var oldSession = servers[instanceId];
                if (oldSession != session)
                    return false;
            }
            return true;
        }

        public void RemoveServer(int instanceId)
        {
            servers.Remove(instanceId);
        }
        int index = 0;
        Dictionary<int, Session> servers = new Dictionary<int, Session>();
    }
}
