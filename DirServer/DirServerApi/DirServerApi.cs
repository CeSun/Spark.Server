using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DirServerApi
{
    struct DirServerConfig
    {
        public string Host;
        public int Port;
    }
    class DirServerApi
    {
        List<TcpClient> sockets = new List<TcpClient>();
        public void Init(List<DirServerConfig> dirServerConfigs)
        {
            foreach (var cfg in dirServerConfigs)
            {
                var tcpClient = new TcpClient();
                tcpClient.Connect(cfg.Host, cfg.Port);
            }
        }

        public void Update()
        {
            
        }

        public void Fini()
        {

        }
    }
}
