using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirServer
{
    class ServerSet
    {
        Dictionary<int, ServerList> servers = new Dictionary<int, ServerList>();

        public bool InsertServer(int zone, int instance, IPEndPoint iPEndPoint)
        {
            var serverList = servers.GetValueOrDefault(zone);
            if (serverList == null)
            {
                serverList = new ServerList();
                servers.Add(zone, serverList);
            }
            return serverList.InsertServer(instance, iPEndPoint);
        }

        public void DeleteServer(int zone, int instance)
        {
            var serverList = servers.GetValueOrDefault(zone);
            serverList?.DeleteServer(instance);
        }

        public IPEndPoint GetServer(int zone, int instance)
        {
            var server = servers.GetValueOrDefault(zone);
            if (server == null)
                return null;
            return server.GetServer(instance);
        }
        public (List<(int, IPEndPoint)>, int) GetServers(int zone)
        {
            var server = servers.GetValueOrDefault(zone);
            if (server == null)
                return (new List<(int id, IPEndPoint ip)>(), 0);
            return server.GetAllServer();
        }
    }
}
