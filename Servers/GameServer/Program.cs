using Frame;
using GameServer;

var builder = ServerBuilder.CreateBuilder(args);

var app = builder.Build<ServerApplication>();

app.Run();

Span<byte> a = new byte[] { 1};
var head = Protocol.Head.Deserialize(a.ToArray());


