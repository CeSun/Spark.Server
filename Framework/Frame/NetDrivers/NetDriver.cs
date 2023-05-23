using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Frame.NetDrivers;

public class NetDriver
{
    public Action<Session>? ConnectedAction;
    public Action<Session>? DisconnectedAction;
    public delegate void ReceiveDelegate(Session session, Span<byte> buffer);
    public ReceiveDelegate? ReceiveAction;
}


public class Session
{
    public required TcpClient Client;

}
