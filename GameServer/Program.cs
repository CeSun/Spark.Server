using System;
using System.Threading.Tasks;

namespace GameServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GameServer gameServer = new GameServer();
            await gameServer.RunAsync();
        }
    }
}
