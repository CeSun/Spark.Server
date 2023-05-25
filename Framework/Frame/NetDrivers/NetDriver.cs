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
    protected void ProcessData(Session session)
    {
        if (session.Length > sizeof(int) )
        {
            var PackLen = BitConverter.ToInt32(session.ReceiveBuffer);
            var BufferLen = session.Length;
            while(BufferLen >= PackLen)
            {
                var pack = session.ReceiveBuffer.AsSpan(session.Length - BufferLen, PackLen);
                InternalReceiveData(session, pack);
                BufferLen = BufferLen - PackLen;
            }
            Array.Copy(session.ReceiveBuffer, session.Length - BufferLen, session.ReceiveBuffer, 0, BufferLen);
            session.Length = BufferLen;
        }
    }

    protected virtual void InternalReceiveData(Session session, Span<byte> buffer)
    {
        ReceiveAction?.Invoke(session, buffer);
    }
}


public class Session
{
    public Session()
    {
        ReceiveBuffer = new byte[1024];
        Length = 0;
    }
    public byte[] ReceiveBuffer { get; private set; }
    
    internal int Length { get; set; }
    public virtual void SendAsync(byte[] buffer)
    {

    }
}
