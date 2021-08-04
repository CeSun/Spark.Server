using DynamicXML;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    /// <summary>
    /// 服务基类
    /// </summary>
    /// <typeparam name="SubT">子类</typeparam>
    public abstract class ServerApp <SubT> where SubT : ServerApp<SubT>, new()
    {
        
        // 配置文件
        protected abstract string ConfPath { get; }
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

        protected dynamic Config { get; set; }
        public static void Start()
        {
            Instance = new SubT();
            try
            {
                Instance.Init();
                while (true)
                {
                    Instance.Update();
                }
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Instance.Fini();
        }
        public static SubT Instance {  get; private set; }
        SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        protected void Init()
        {
            if (ConfPath != null) {
                StreamReader streamReader = new StreamReader(ConfPath);
                var xml = streamReader.ReadToEnd();
                streamReader.Close();
                dynamic cfg = new DynamicXml(xml);
                Config = cfg.Config;
            }
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            OnInit();
        }

        protected void Update()
        {
            SyncContext.Update();
            Thread.Sleep(0);
            OnUpdate();
        }

        protected void Fini()
        {
            OnFini();
        }

    }
}
