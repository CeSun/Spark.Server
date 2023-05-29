using Microsoft.CodeAnalysis;
using System;

namespace Dispatcher
{

    [Generator]
    public class DispatcherSourceGenerator : ISourceGenerator
    {
        ModuleSourceGenerator ModuleSourceGenerator = new ModuleSourceGenerator();
        ProtoDispatcherSourceGenerator ProtoDispatcherSourceGenerator = new ProtoDispatcherSourceGenerator();
        public void Execute(GeneratorExecutionContext context)
        {
            ModuleSourceGenerator.Execute(context);
            ProtoDispatcherSourceGenerator.Execute(context);
            context.AddSource("ServerApplication.g.cs", ServerClassSourceCode);
            context.AddSource("RouteAttribute.g.cs", Route);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            ModuleSourceGenerator.Initialize(context);
            ProtoDispatcherSourceGenerator.Initialize(context);
        }

        string ServerClassSourceCode = 
$@"using Frame;
using Frame.NetDrivers;
using GameServer.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame;
public partial class ServerApplication : Application
{{
    public event Action<Session> ClientConnectedEvent;
    public event Action<Session> ClientDisonnectedEvent;
    public event ReceiveDelegate ClientReceiveDataEvent;

    protected override void OnClientConnected(Session session)
    {{
        ClientConnectedEvent.Invoke(session);
    }}

    protected override void OnClientDisconnected(Session session)
    {{
        ClientDisonnectedEvent.Invoke(session);
    }}

    protected override void OnClientReceiveData(Session session, Span<byte> data)
    {{
        var PackLen = BitConverter.ToInt32(data);
        var HeadLen = BitConverter.ToInt32(data.Slice(sizeof(Int32)));
        var HeadData = data.Slice(2 * sizeof(Int32), HeadLen - sizeof(Int32));
        var Head = Protocol.Head.Deserialize(HeadData.ToArray());

        switch(Head.MsgId)
        {{
            case Protocol.MsgId.LoginReq:
            {{
                break;
            }}

            default:
            {{
                break;
            }}
        }}
    
        ClientReceiveDataEvent.Invoke(session, data);
    }}
/*
    protected override void Dispatcher(TReq req, Func<TReq, Task<TRsp>> func, Session session)
    {{
        TRsp rsp = await func(req);
    }}
*/
}}

";
        public static string Route = $@"
";
    }

}
