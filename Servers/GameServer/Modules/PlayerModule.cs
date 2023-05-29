using Frame;
using Frame.Attributes;
using Frame.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Protocol;
using System.Diagnostics.CodeAnalysis;

namespace GameServer.Modules;


[Module("PlayerModule")]
public class PlayerModule : ILifeCycle
{
    public required ServerApplication Server {
        get;
        init;
    }

    [Route(Protocol.LoginReq, Protocol.LoginRsp)]
    public int Login(int a)
    {
        return 0;
    }

    public void OnStart()
    {
       
    }

    public void OnStop()
    {
        Console.WriteLine("OnStop");
    }

}
