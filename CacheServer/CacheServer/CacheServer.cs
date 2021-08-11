using CacheServer.Modules;
using Frame;
using System;

namespace CacheServer
{
    
    class CacheServer : ServerBase<CacheServer, Config>
    {
        protected override string ConfPath => "../CacheServerConfig.xml";

        protected override void OnInit()
        {
           Redis.Instance.Init(Config.Redis);
            Mysql.Instance.Init(Config.Mysql);
            Timer.Instance.Init();
            Timer.Instance.SetInterval(3000, () => { Console.WriteLine("123"); });
        }

        protected void DataHandler( byte[] data)
        {
        }

        protected override void OnUpdate()
        {
            // Redis.Instance.Update();
            //  Mysql.Instance.Update();
            Timer.Instance.Update();
        }
        protected override void OnFini()
        {
            Redis.Instance.Fini();
            Mysql.Instance.Update();
        }

    }
}
