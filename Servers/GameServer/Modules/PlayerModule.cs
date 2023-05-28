using Frame;
using Frame.Attributes;
using Frame.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Modules;

[Module("PlayerModule")]
public class PlayerModule : ILifeCycle
{
    public required ServerApplication Server {
        get;
        init;
    }

    [Route<int>(1,2)]
    public int Login(int a)
    {
        return 0;
    }

    public void OnStart()
    {
        Console.WriteLine("OnStart");
    }

    public void OnStop()
    {
        Console.WriteLine("OnStop");
    }

    public void OnUpdate()
    {
        Console.WriteLine("OnUpdate");
    }
}
