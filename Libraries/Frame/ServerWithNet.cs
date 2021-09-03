using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public abstract class ServerWithNet : Application
    {
        Network network = new Network();
        protected sealed override async Task OnFiniAsync()
        {
            await network.Fini();
        }

        protected sealed override async Task OnInitAsync()
        {
            await network.InitAsync(this, new Network.Callbacks { dataHandler = DataHandler, disconnectedHandler = DisconnectHandler, connectedHandler = ConnectHandler});
            await network.RunAsync();
        }

        protected abstract Task DataHandler(ISession session, byte[] data);
        protected abstract Task DisconnectHandler (ISession session);
        protected abstract Task ConnectHandler(ISession session);

    }
}
