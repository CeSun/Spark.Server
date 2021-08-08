using CacheServer.Modules;
using Frame;

namespace CacheServer
{
    class CacheServer : ServerBase<CacheServer>
    {
        protected override string ConfPath => "../CacheServerConfig.xml";

        protected override void OnInit()
        {
            Redis.Instance.Init(default);
            Mysql.Instance.Init(default);
        }

        protected void DataHandler( byte[] data)
        {
        }

        protected override void OnUpdate()
        {
            Redis.Instance.Update();
            Mysql.Instance.Update();
        }
        protected override void OnFini()
        {
            Redis.Instance.Fini();
            Mysql.Instance.Update();
        }

    }
}
