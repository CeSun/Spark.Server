namespace Frame
{
    public class Application
    {
        private SingleThreadSynchronizationContext SyncContext = new SingleThreadSynchronizationContext();
        public required ConfigBase Config;

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
        }
        public void Update()
        {
            SyncContext.Update();
        }
        public void Stop()
        {

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