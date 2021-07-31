using DataBase.Tables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Player
{
    public class PlayerMngr
    {
        private Dictionary<ulong, Player> playerPool { get; set; }

        public PlayerMngr()
        {
            playerPool = new Dictionary<ulong, Player>();
            
        }

        public void AddPlayer(ulong sessionId, Player player)
        {
            playerPool.Add(sessionId, player);
        }

        public Player GetPlayer(ulong sessionId)
        {
            return playerPool.GetValueOrDefault(sessionId);
        }
        public void Init()
        {

        }
        public void Update()
        {
            List<ulong> waitDeletePlayer = new List<ulong>();
            foreach (var playerPair in playerPool)
            {
                playerPair.Value.Update();
                if (playerPair.Value.IsDisConnected == true)
                {
                    waitDeletePlayer.Add(playerPair.Key);
                }
            }
            waitDeletePlayer.ForEach(x => playerPool.Remove(x));
        }

        public void Fini()
        {
            foreach(var playerPair in playerPool)
            {
                playerPair.Value.Fini();
            }
            playerPool.Clear();
        }

    }

}
