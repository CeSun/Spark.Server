using Frame;
using GameServer.Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataBase;
using GameServer.Module;

namespace GameServer
{
    public class Server : ServerAppWithNet<Server>
    {
        public PlayerMngr playerPool = new PlayerMngr();
        public int Zone { get { return 1; } }
        public int InstanceId { get { return 1; } }
        public UinMngr UinMngr { get; private set; }

        protected override string ConfPath => "../GameServerConfig.xml";

        protected override void OnInit() 
        {
            base.OnInit();
            UinMngr = new UinMngr();
            Database.Init();
            playerPool.Init();
            UinMngr.Init(Zone); 
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            Database.Update();
            playerPool.Update();
            UinMngr.Update();
        }
        protected override void OnFini()
        {
            base.OnFini();
            Database.Fini();
            playerPool?.Fini();
            UinMngr?.Fini();
        }

        protected async override Task OnHandlerData(Session session, byte[] data)
        {
            var player = playerPool.GetPlayer(session.SessionId);
           if (player == null)
           {
                player = new Player.Player(session);
                player.Init();
                playerPool.AddPlayer(session.SessionId, player);
           }
           await player.processData(data);
        }

        protected override void OnConnect(Session session)
        {

        }

        protected override void OnDisconnect(Session session)
        {

        }
    }
}
