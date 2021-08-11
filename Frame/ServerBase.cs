using DynamicXML;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Frame
{
    /// <summary>
    /// 服务基类
    /// </summary>
    /// <typeparam name="SubT">子类</typeparam>
    public abstract class ServerBase<SubT, TConfig> where SubT : ServerBase<SubT, TConfig>, new() where TConfig: BaseConfig
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

        protected TConfig Config { get; set; }
        public static void Start()
        {
            Instance = new SubT();
            try
            {
                Instance.Init();
                ulong frameNo = 0;
                var start = DateTime.Now;
                while (true)
                {
                    TimeMngr.Instance.Update();
                    frameNo++;
                    Instance.Update();
                    if ((DateTime.Now - start).TotalMilliseconds >= 1000)
                    {
                        Console.WriteLine(frameNo);
                        start = DateTime.Now;
                        frameNo = 0;
                    }
                }
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Instance.Fini();
        }
        public static SubT Instance {  get; private set; }
        protected SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        protected void PostAsyncTask(Action action)
        {
            SyncContext.Post(state => action(), null);
        }
        protected void Init()
        {
            if (ConfPath != null) {
                StreamReader streamReader = new StreamReader(ConfPath);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TConfig));
                Config = xmlSerializer.Deserialize(streamReader) as TConfig;
                streamReader.Close();
            }
            SynchronizationContext.SetSynchronizationContext(SyncContext);
            TimeMngr.Instance.Init(Config.Time.Zone);
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

    /// <summary>
    /// 服务配置基类
    /// </summary>
    public class BaseConfig
    {
        public TimeConfig Time;
        public struct TimeConfig
        {
            public int Zone;
        }
    }
}
