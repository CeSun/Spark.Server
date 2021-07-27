using Frame;
using GameServer.Player;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    public class Server
    {
        NetworkMngr netWorkMngr = new NetworkMngr();
        PlayerPool playerPool = new PlayerPool();
        SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        public static void Start()
        {
            Server gameServer = new Server();
            try
            {
                gameServer.Init();
                while(true)
                {
                    gameServer.Update();
                }
            }
            catch { }
            gameServer.Fini();
        }

        void Init()
        {
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            netWorkMngr.Init(DataHandler);
        }

        async Task DataHandler(Session session, byte[] data)
        {
           var player = playerPool.playerPool.GetValueOrDefault(session.SessionId);
           if (player == null)
           {
                player = new Player.Player(session);
                playerPool.playerPool.Add(session.SessionId, player);
           }
           await player.processData(data);
        }

        void Update()
        {
            SyncContext.Update();
            netWorkMngr.Update();
            Thread.Sleep(0);
        }

        void Fini()
        {
            netWorkMngr.Fini();
        }
      
    }
}
