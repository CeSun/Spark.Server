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
            _ = UpdateAsync();
        }
        private ulong AddNums = 100;
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
            var ret = await TUin.QueryAync(zone);
            TUin uin = null;
            if (ret.Error != DBError.Success)
            {
                if (ret.Error != DBError.IsNotExisted)
                    throw new Exception(ret.Error + "");
                uin = TUin.New();
                uin.Base.Nums = 0;
                uin.Base.Zone = zone;
                var ret3 = await uin.SaveAync();
                if (ret3 != DBError.Success)
                    throw new Exception(ret3 + "");

            }else
            {
                uin = ret.Row;
            }
            var num1 = uin.Base.Nums;
            uin.Base.Nums += AddNums;
            var ret2 = await uin.SaveAync();
            if (ret2 != DBError.Success)
                throw new Exception(ret2 + "");
            StartNum = num1;
            EndNum = ret.Row.Base.Nums;
        }
        List<TaskCompletionSource> tasks = new List<TaskCompletionSource>();
        private async Task UpdateAsync()
        {
            await Task.Delay(3000);
            for(int i = 0;i < 10000; i ++)
            {
                try
                {
                    _ =  GetUinAsync();
                } catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
        Task updateTask = null;
        public async Task<ulong> GetUinAsync()
        {
            try
            {
                if (StartNum == EndNum)
                {
                    if (updateTask == null)
                        updateTask = updateMngrAsync();
                    await updateTask;
                    updateTask = null;
                
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                updateTask = null;
                return 0;
            }
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
