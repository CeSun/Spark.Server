using Frame;
using GameServer.Player;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    public class Server : ServerApp<Server>
    {
        PlayerPool playerPool = new PlayerPool();

        protected override void OnInit()
        {

        }
        protected override void OnUpdate()
        {
            playerPool.Update();
        }
        protected override void OnFini()
        {
            playerPool.Fini();
        }

        protected async override Task OnHandlerData(Session session, byte[] data)
        {
           var player = playerPool.playerPool.GetValueOrDefault(session.SessionId);
           if (player == null)
           {
                player = new Player.Player(session);
                playerPool.playerPool.Add(session.SessionId, player);
           }
           await player.processData(data);
        }


    }
}
