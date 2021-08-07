using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer
{
    class CacheServer : ServerBase<CacheServer>
    {
        protected override string ConfPath => "../CacheServerConfig.xml";
        protected override void OnFini()
        {
        }

        protected override void OnInit()
        {
        }

        protected void DataHandler( byte[] data)
        {

        }

        protected override void OnUpdate()
        {
        }
    }
}
