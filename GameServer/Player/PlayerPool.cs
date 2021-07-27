using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Player
{
    public class PlayerPool
    {
        public Dictionary<UInt64, Player> playerPool { get; private set; }

        public PlayerPool()
        {
            playerPool = new Dictionary<UInt64, Player>();
        }

        public void Update()
        {
            List<UInt64> waitDeletePlayer = new List<UInt64>();
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
