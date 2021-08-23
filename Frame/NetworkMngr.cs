using Frame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    public delegate void DataHandler(Session session, byte[] data);
    public delegate void ConnectHandler(Session session);

    /// <summary>
    /// 网络管理类
    /// </summary>
    class NetworkMngr
    {
        DataHandler dataHandler;
        ConnectHandler connectHandler;
        ConnectHandler disconnectHandler;
        TcpListener tcpListener = null;

        ulong iditer = 0;
        public void Init(IPEndPoint ListenIpEndPoint, DataHandler dataHandler, ConnectHandler connectHandler, ConnectHandler disconnectHandler)
        {
            this.dataHandler = dataHandler;
            this.connectHandler = connectHandler;
            this.disconnectHandler = disconnectHandler;
            tcpListener = new TcpListener(ListenIpEndPoint);
            tcpListener.Start();
            CoroutineUtil.Instance.New(StartAsync);
        }
        private async Task StartAsync()
        {
            while (true)
            {
                    try
                    {

                        var client = await tcpListener.AcceptTcpClientAsync();

                    CoroutineUtil.Instance.New(async () => await ProcessAsync(client));
                    } catch (Exception ex)
                    {

                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    }
            }
        }
        public void Update()
        {
            // nothing
        }
        private async Task ProcessAsync(TcpClient tcpClient)
        {

            Session session = new Session { client = tcpClient, SessionId = ++iditer, latestRec = TimeMngr.Instance.RealTimestamp };
            CoroutineUtil.Instance.New(() => connectHandler(session));
            try
            {
                var buffer = new byte[1024 * 1024];
                var StartIndex = 0;
                var stream = tcpClient.GetStream();
                int len = 0;
                while((len = await stream.ReadAsync(buffer, StartIndex, buffer.Length - StartIndex)) != 0)
                {
                    StartIndex = StartIndex + len;

                    if (StartIndex >= 4)
                    {
                        var PackLenBits = buffer.Take(4).ToArray();
                        Array.Reverse(PackLenBits);
                        var PackLen = BitConverter.ToInt32(PackLenBits, 0);
                        if (PackLen >= 1024 * 1024)
                        {
                            break;
                        }
                        if (StartIndex >= PackLen)
                        {
                            var data = buffer.Take(PackLen).ToArray();
                            session.latestRec = TimeMngr.Instance.RealTimestamp;
                            dataHandler(session, data);
                            data = buffer.Skip(PackLen).Take(StartIndex - PackLen).ToArray();
                            Array.Copy(data, buffer, data.Length);
                            StartIndex = StartIndex - PackLen;
                        }
                    }
                    }
            } catch (Exception ex)
            {
                // Console.WriteLine(ex.Message);
                // Console.WriteLine(ex.StackTrace);
            }
            CoroutineUtil.Instance.New(async () => disconnectHandler(session));
        }
        public void Fini()
        {
            
        }
    }
    public class Session : ISession
    {
        public TcpClient client;
        public ulong SessionId;
        public long latestRec;
        private object process;
        public TProcess GetProcess<TProcess>(){
            if (this.process == null)
                return default;
            return (TProcess)this.process;
        }
        public void SetProcess<TProcess>(TProcess value)
        {
            process = value;
        }
        public async Task SendAsync(byte[] data)
        {
            var stream = client.GetStream();
            await stream.WriteAsync(data);
        }

    }

    public interface ISession
    {
        Task SendAsync(byte[] data);
    }
}
