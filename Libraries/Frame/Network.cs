using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Frame
{
    public class Network
    {

        Application application = null;

        public struct Callbacks
        {
            public delegate Task DataHandler(ISession session, byte[] data);
            public delegate Task ConnectedHandler(ISession session);
            public DataHandler dataHandler;
            public ConnectedHandler connectedHandler;
            public ConnectedHandler disconnectedHandler;
        }
        Callbacks callbacks;

        public async Task InitAsync(Application application, Callbacks callbacks)
        {
            this.application = application;
            this.callbacks = callbacks;
        }
        public async Task RunAsync()
        {
            try
            {
                TcpListener listener = TcpListener.Create(2007);
                listener.Start();
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = AcceptAsync(client);
                }
            } catch
            {
                application.Single.Post(ESingle.Exception);
            }
        }

        private async Task AcceptAsync(TcpClient client)
        {
            var session = new Session { Client = client };
            try
            {
                await callbacks.connectedHandler(session);
                Memory<byte> buffer = new Memory<byte>();
                var stream = client.GetStream();
                int len = 0;
                while ((len = await stream.ReadAsync(buffer)) != 0)
                {

                }

            } catch
            {

            }
            await callbacks.disconnectedHandler(session);
        }

        public async Task Fini()
        {

        }
    }
    public interface ISession
    {
        public Task SendToClientAsync(byte[] data);
    }
    class Session : ISession
    {
        public TcpClient Client {  get; set; }
        public async Task SendToClientAsync(byte[] data)
        {
            await Client.GetStream().WriteAsync(data);
        }
    }

}
