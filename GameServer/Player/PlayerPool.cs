using DataBase.Tables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Player
{
    public class PlayerPool
    {
        public Dictionary<UInt64, Player> playerPool { get; private set; }

        public PlayerPool()
        {
            playerPool = new Dictionary<UInt64, Player>();
        }
        public void Init()
        {

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

    public class UinMngr { 
        public void Init()
        {
            state = State.Loading;
            updateMngrAsync();
        }
        private int AddNums = 100;
        private ulong StartNum = 0;
        private ulong EndNum = 0;
        enum State
        {
            Loaded,
            Loading
        }
        private State state;
        private async Task updateMngrAsync()
        {
            var exception =  new Exception("error");
            var ret = await TUin.QueryAync(Server.Instance.Zone);
            if (ret.Error == DataBase.DBError.IsNotExisted)
            {
                var uin = TUin.New();
                uin.Value.Zone = Server.Instance.Zone;
                uin.Value.Nums = 0;
                var ret2 = await uin.SaveAync();
                if (ret2 == DataBase.DBError.IsExisted)
                {
                    await Task.Delay((int)(Random.Shared.NextDouble() * 1000));
                    ret = await TUin.QueryAync(Server.Instance.Zone);
                    if (ret.Error != DataBase.DBError.Success)
                    {
                        state = State.Loaded;
                        foreach (var tcs in tasks)
                        {
                            tcs.SetException(exception);
                        }
                        tasks.Clear();
                        throw exception;
                    }
                }
                else if (ret.Error != DataBase.DBError.Success)
                {
                    state = State.Loaded;
                    foreach (var tcs in tasks)
                    {
                        tcs.SetException(exception);
                    }
                    tasks.Clear();
                    throw exception;
                }
            }
            this.StartNum = ret.Row.Value.Nums;
            ret.Row.Value.Nums += (ulong)AddNums;
            this.EndNum = ret.Row.Value.Nums;
            var ret3 = await ret.Row.SaveAync();
            if (ret3 != DataBase.DBError.Success)
            {
                state = State.Loaded;
                foreach (var tcs in tasks)
                {
                    tcs.SetException(exception);
                }
                tasks.Clear();
                throw exception;
            }
            state = State.Loaded;
            foreach(var tcs in tasks)
            {
                tcs.SetResult();
            }
            tasks.Clear();
        }
        List<TaskCompletionSource> tasks = new List<TaskCompletionSource>();
        private Task UpdateAsync()
        {
            TaskCompletionSource tcs = new TaskCompletionSource();
            if (StartNum == EndNum)
            {
                if (state == State.Loaded)
                {
                    if (StartNum == EndNum)
                    {
                        state = State.Loading;
                        tasks.Add(tcs);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                        updateMngrAsync();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    }
                    else
                    {
                        tcs.SetResult();
                    }
                }
            }
            else
            {
                tcs.SetResult();
            }
            return tcs.Task;
        }

        public async Task<ulong> GetUinAsync()
        {
            await UpdateAsync();
            return ++StartNum;
        }

        
        public void Update()
        {

        }

        public void Fini()
        {

        }
    }
}
