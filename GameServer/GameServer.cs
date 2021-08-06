using Frame;
using GameServer.Player;
using System.Threading.Tasks;
using DataBase;
using GameServer.Module;
using System;
using System.Diagnostics;

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
            Database.Init(Config.Mysql);
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
        protected override void OnHandlerData(Session session, byte[] data)
        {
            var player = session.GetProcess<Player.Player>();
            if (player == null)
            {
                player = new Player.Player(session);
                playerPool.AddPlayer(session.SessionId, player);
                session.SetProcess(player);
                player.Init();
            }
            player.processData(data);

        }
    }
}
