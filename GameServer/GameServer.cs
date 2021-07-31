using Frame;
using GameServer.Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataBase;

namespace GameServer
{
    public class Server : ServerApp<Server>
    {
        public PlayerPool playerPool = new PlayerPool();
        public int Zone { get { return 1; } }
        public int InstanceId { get { return 1; } }
        public UinMngr UinMngr { get; private set; }
        protected override void OnInit()
        {
            UinMngr = new UinMngr();
            Database.Init();
            playerPool.Init();
            UinMngr.Init(); 
        }
        protected override void OnUpdate()
        {
            Database.Update();
            playerPool.Update();
            UinMngr.Update();
        }
        protected override void OnFini()
        {
            Database.Fini();
            playerPool.Fini();
            UinMngr.Fini();
        }

        protected async override Task OnHandlerData(Session session, byte[] data)
        {
           var player = playerPool.playerPool.GetValueOrDefault(session.SessionId);
           if (player == null)
           {
                player = new Player.Player(session);
                player.Init();
                playerPool.playerPool.Add(session.SessionId, player);
           }
           await player.processData(data);
        }


    }
}
