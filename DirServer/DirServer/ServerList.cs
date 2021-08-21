using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirServer
{
    
    class ServerList
    {
        Dictionary<int, IPEndPoint> servers = new Dictionary<int, IPEndPoint>();
        int version = 0;

        public bool InsertServer (int instance, IPEndPoint iPEndPoint)
        {
            if (servers.ContainsKey(instance))
                return false;
            servers[instance] = iPEndPoint;
            version++;
            return true;
        }

        public void DeleteServer(int instance)
        {
            if (servers.ContainsKey(instance))
            {
                servers.Remove(instance);
                version++;
            }
        }

        public IPEndPoint GetServer(int instance)
        {
            return servers.GetValueOrDefault(instance);
        }

        public (List<(int, IPEndPoint)>, int) GetAllServer()
        {
            List<(int, IPEndPoint)> rtl = new List<(int, IPEndPoint)>();
            foreach(var pair in servers)
            {
                rtl.Add((pair.Key, pair.Value));
            }
            return (rtl, version);
        }

        
    }
}
