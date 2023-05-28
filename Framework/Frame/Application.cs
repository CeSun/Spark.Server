using Frame.Interfaces;
using Frame.NetDrivers;

namespace Frame
{
    public abstract class Application : ILifeCycle
    {
        private SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        internal ConfigBase? Config;
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
            if (GetConfig<ConfigBase>().ServerMode == ServerMode.Server)
            {
                var ServerDriver = new ServerNetDriver()
                {
                    ConnectedAction = OnClientConnected,
                    DisconnectedAction = OnClientDisconnected,
                    ReceiveAction = OnClientReceiveData
                };
                ServerDriver.Init(GetConfig<ConfigBase>().HostAndPort);
                NetDriver = ServerDriver;
            }
            OnStart();
        }

        public void Update()
        {
            SyncContext.Update();
            OnUpdate();
        }
        public void Stop()
        {
            NetDriver?.Fini();
            OnStop();
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

        protected abstract void OnClientConnected(Session session);
        protected abstract void OnClientDisconnected(Session session);
        protected abstract void OnClientReceiveData(Session session, Span<byte> data);

        public virtual void OnStart()
        {
        }

        public virtual void OnStop()
        {
        }

        public virtual void OnUpdate()
        {
        }
    }
}