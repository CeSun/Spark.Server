using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Frame.NetDrivers;

public abstract class NetDriver
{
    public Action<Session>? ConnectedAction;
    public Action<Session>? DisconnectedAction;
    public delegate void ReceiveDelegate(Session session, Span<byte> buffer);
    public ReceiveDelegate? ReceiveAction;
    protected bool IsExit = false;
    public void Fini()
    {
        IsExit = true;
    }
}


public class Session
{
    public required TcpClient Client;

}
