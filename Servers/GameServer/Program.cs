using Frame;
using GameServer;

var builder = ServerBuilder.CreateBuilder(args);

var app = builder.Build<GameServerApplication>();

app.Run();
