﻿using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    public abstract class ServerApp <SubT> where SubT : ServerApp<SubT>, new()
    {
        /// <summary>
        /// tcp协议处理
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="data">二进制数据</param>
        /// <returns>无，可以await</returns>
        protected abstract Task OnHandlerData(Session session, byte[] data);

        /// <summary>
        /// 框架启动时调用
        /// </summary>
        protected abstract void OnInit();
        /// <summary>
        /// 每帧调用
        /// </summary>
        protected abstract void OnUpdate();
        /// <summary>
        /// 框架结束时调用
        /// </summary>
        protected abstract void OnFini();
        public static void Start()
        {
            SubT gameServer = new SubT();
            try
            {
                gameServer.Init();
                while (true)
                {
                    gameServer.Update();
                }
            }
            catch { }
            gameServer.Fini();
        }

        NetworkMngr netWorkMngr = new NetworkMngr();
        SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        void Init()
        {
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            netWorkMngr.Init(OnHandlerData);
            OnInit();
        }

        void Update()
        {
            SyncContext.Update();
            netWorkMngr.Update();
            Thread.Sleep(0);
            OnUpdate();
        }
        
        void Fini()
        {
            netWorkMngr.Fini();
            OnFini();
        }


    }
}
