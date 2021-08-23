using Frame;
using GameServer.Player;
using System.Threading.Tasks;
using GameServer.Module;
using System;
using System.Diagnostics;
using ProxyServerApi;
using System.Xml.Serialization;
using CacheServerApi;

namespace GameServer
{
    public class Config : BaseNetConfig
    {
        [XmlArray("IpAndPoint"), XmlArrayItem("value")]
        public string[] IpAndPoint;
    }
    public class Server : ServerBaseWithNet<Server, Config>
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
            playerMngr.Init();
            ProxyModule.Instance.Init(Config.IpAndPoint, new ServerInfo {id=1, name="GameServer", zone = 1 });
            CacheServerModule.Instance.Init(ProxyModule.Instance);

            UinMngr.Init(Zone);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            playerMngr?.Update();
            UinMngr?.Update();
            ProxyModule.Instance?.Update();
            CacheServerModule.Instance.Update();

        }
        protected override void OnFini()
        {
            base.OnFini();
            playerMngr?.Fini();
            UinMngr?.Fini();
            ProxyModule.Instance?.Fini();
            CacheServerModule.Instance.Fini();
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
