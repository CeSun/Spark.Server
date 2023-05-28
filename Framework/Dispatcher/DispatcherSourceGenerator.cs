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
            context.AddSource("ServerApplication.cs", ServerClassSourceCode);
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
    protected override void OnClientConnected(Session session)
    {{
    }}

    protected override void OnClientDisconnected(Session session)
    {{
    }}

    protected override void OnClientReceiveData(Session session, Span<byte> data)
    {{
    }}
}}

";
    }

}
