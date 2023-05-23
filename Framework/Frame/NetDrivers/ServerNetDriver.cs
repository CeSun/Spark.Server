using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Frame.NetDrivers;

public class ServerNetDriver : NetDriver
{
    HostAndPort HostAndPort = new HostAndPort();
    public void Init(HostAndPort config)
    {
        ServerMain();
    }

    public async void ServerMain()
    {
        TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Parse(HostAndPort.Host), HostAndPort.Port));
        listener.Start();
        while (IsExit == false)
        {
            var client = listener.AcceptTcpClient();
            ProcessClient(client);
        }
    }

    public async void ProcessClient(TcpClient client)
    {
        client.GetStream();
    }

}
