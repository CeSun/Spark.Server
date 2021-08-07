using Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer
{
    class CacheServer : ServerBaseWithNet<CacheServer>
    {
        protected override string ConfPath => throw new NotImplementedException();

        protected override void OnHandlerConnected(Session session)
        {
        }

        protected override void OnHandlerData(Session session, byte[] data)
        {
        }

        protected override void OnHandlerDisconnected(Session session)
        {
        }
    }
}
