using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame;

public class ConfigBase
{
    public ConfigBase()
    {
        HostAndPort = new HostAndPort();
    }

    public HostAndPort HostAndPort { get; set; }

    public ServerMode ServerMode { get; set; }
}

public enum ServerMode
{
    Proxy,
    Server
}
public class HostAndPort
{
    public HostAndPort()
    {
        Host = "localhost";
        Port = 7766;
    }
    public string Host { get; set; }
    public int Port { get; set; }

}
