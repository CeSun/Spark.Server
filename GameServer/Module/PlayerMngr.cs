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

        public void Remove(ulong sessionId)
        {
            var player = playerPool.GetValueOrDefault(sessionId);
            player?.Disconnected();
            playerPool.Remove(sessionId);
        }
        public void AddPlayer(ulong sessionId, Player player)
        {
            playerPool.Add(sessionId, player);
        }

        public Player GetPlayer(ulong sessionId)
        {
            if (playerPool.ContainsKey(sessionId))
                return  playerPool[sessionId];
            return null;
        }
        public void Init()
        {

        }
        public void Update()
        {/*
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
            */
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
