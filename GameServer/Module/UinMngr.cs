using CacheServerApi.Tables;
using ProxyServerApi.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Module
{
    public class UinMngr
    {
        private int zone;
        public void Init(int zone)
        {
            this.zone = zone;
            state = State.Loading;
            _ = updateMngrAsync();
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
            try
            {
                await Task.Delay(5000);
                var exception = new Exception("error");
                var ret = await TUin.QueryAync(zone);
                if (ret.Error == DBError.IsNotExisted)
                {
                    var uin = TUin.New();
                    uin.Base.Zone = zone;
                    uin.Base.Nums = 0;
                    var ret2 = await uin.SaveAync();
                    if (ret2 == DBError.IsExisted)
                    {
                        Random random = new Random(DateTime.Now.Second);
                        await Task.Delay((int)(random.NextDouble() * 1000));
                        ret = await TUin.QueryAync(Server.Instance.Zone);
                        if (ret.Error != DBError.Success)
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
                    else if (ret.Error != DBError.Success)
                    {
                        state = State.Loaded;
                        foreach (var tcs in tasks)
                        {
                            tcs.SetException(exception);
                        }
                        tasks.Clear();
                        throw exception;
                    }
                    else if (ret.Error != DBError.Success)
                    {
                        throw exception;
                    }
                }
                else if (ret.Error != DBError.Success)
                {
                    throw exception;
                }
                this.StartNum = ret.Row.Base.Nums;
                ret.Row.Base.Nums += (ulong)AddNums;
                this.EndNum = ret.Row.Base.Nums;
                var ret3 = await ret.Row.SaveAync();
                if (ret3 != DBError.Success)
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
                foreach (var tcs in tasks)
                {
                    tcs.SetResult();
                }
                tasks.Clear();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            
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
                        _ = updateMngrAsync();
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
