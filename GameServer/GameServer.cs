using Frame;
using GameServer.Player;
using System.Threading.Tasks;
using DataBase;
using GameServer.Module;
using System;
using System.Diagnostics;
using ProxyServerApi;
using System.Xml.Serialization;

namespace GameServer
{
    public class Config : BaseNetConfig
    {
        public MysqlConfig Mysql;
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
        ProxyModule proxyModule = new ProxyModule();

        protected override void OnInit() 
        {
            base.OnInit();
            UinMngr = new UinMngr();
            Config.Mysql.PoolSize = 10;
            Database.Init(Config.Mysql);
            playerMngr.Init();
            UinMngr.Init(Zone);
            proxyModule.Init(Config.IpAndPoint);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Database.Update();
            playerMngr?.Update();
            UinMngr?.Update();
            proxyModule?.Update();

        }
        protected override void OnFini()
        {
            base.OnFini();
            Database.Fini();
            playerMngr?.Fini();
            UinMngr?.Fini();
            proxyModule?.Fini();
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
