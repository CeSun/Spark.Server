using Frame.NetDrivers;

namespace Frame
{
    public class Application
    {
        private SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        public required ConfigBase Config;
        NetDriver? NetDriver;
        public T GetConfig<T>() where T : ConfigBase
        {
            if (Config is T config)
                return config;
            throw new Exception("配置文件类型异常！");
        }
        protected bool IsExit { get; set; }
        public void Start()
        {
            SyncContext.Init();
            if (Config.ServerMode == ServerMode.Server)
            {
                var ServerDriver = new ServerNetDriver();
                ServerDriver.Init(Config.HostAndPort);
                NetDriver = ServerDriver;
            }
        }
        public void Update()
        {
            SyncContext.Update();
        }
        public void Stop()
        {
            NetDriver?.Fini();
        }

        public void Run()
        {
            Start();
            while (IsExit == false)
            {
                Update();
            }
            Stop();
        }
    }
}