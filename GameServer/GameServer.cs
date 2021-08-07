using Frame;
using GameServer.Player;
using System.Threading.Tasks;
using DataBase;
using GameServer.Module;
using System;
using System.Diagnostics;

namespace GameServer
{
    public class Server : ServerBaseWithNet<Server>
    {
        public PlayerMngr playerMngr = new PlayerMngr();
        public int Zone { get { return 1; } }
        public int InstanceId { get { return 1; } }
        public UinMngr UinMngr { get; private set; }

        protected override string ConfPath => "../GameServerConfig.xml";

        protected override void OnInit() 
        {
            base.OnInit();
            UinMngr = new UinMngr();
            Database.Init(Config.Mysql);
            playerMngr.Init();
            UinMngr.Init(Zone);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Database.Update();
            playerMngr?.Update();
            UinMngr?.Update();
        
        }
        protected override void OnFini()
        {
            base.OnFini();
            Database.Fini();
            playerMngr?.Fini();
            UinMngr?.Fini();
        }
        protected override void OnHandlerData(Session session, byte[] data)
        {
            var player = session.GetProcess<Player.Player>();
            player.processData(data);
        }

        protected override void OnHandlerConnected(Session session)
        {
            var player = new Player.Player(session);
            playerMngr.AddPlayer(session.SessionId, player);
            session.SetProcess(player);
            player.Init();
        }

        protected override void OnHandlerDisconnected(Session session)
        {
            playerMngr.Remove(session.SessionId);
        }
    }
}
