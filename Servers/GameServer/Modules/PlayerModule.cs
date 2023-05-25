using Frame.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Modules;

[Module("PlayerModule")]
public class PlayerModule
{
    [Route<int>(1,2)]
    public int Login(int a)
    {
        return 0;
    }
}
