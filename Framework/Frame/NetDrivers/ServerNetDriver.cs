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
        HostAndPort = config;
        ServerMain();
    }

    public async void ServerMain()
    {
        TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Parse(HostAndPort.Host), HostAndPort.Port));
        listener.Start();
        while (IsExit == false)
        {
            var client = await listener.AcceptTcpClientAsync();
            ProcessClient(client);
        }
        listener.Stop();
    }

    public async void ProcessClient(TcpClient client)
    {
        var session = new ServerNetSession() { TcpClient = client };
        try
        {
            ConnectedAction?.Invoke(session);
            await Task.Yield();
            var stream = client.GetStream();
            Memory<byte> buffer = new Memory<byte>();
            while (IsExit == false)
            {
                var len = await stream.ReadAsync(session.ReceiveBuffer, session.Length, session.ReceiveBuffer.Length - session.Length);
                session.Length += len;
                ProcessData(session);
            }
        } 
        catch (IOException exception)
        {
            DisconnectedAction?.Invoke(session);
            Console.WriteLine(exception.ToString());
        }
        catch (ObjectDisposedException exception)
        {
            DisconnectedAction?.Invoke(session);
            // todo: Debug
            Console.WriteLine(exception.ToString());
        }
    }


}

internal class ServerNetSession : Session {

    internal required TcpClient TcpClient;

    public override async void SendAsync(byte[] buffer)
    {
        await TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
    }
}

