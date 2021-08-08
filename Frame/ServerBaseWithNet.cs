using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public abstract class ServerBaseWithNet<SubT> : ServerBase<SubT> where SubT : ServerBase<SubT>, new()
    {
        NetworkMngr netWorkMngr = new NetworkMngr();
        /// <summary>
        /// tcp协议处理
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="data">二进制数据</param>
        /// <returns>无，可以await</returns>
        protected abstract void OnHandlerData(Session session, byte[] data);
        protected abstract void OnHandlerConnected(Session session);
        protected abstract void OnHandlerDisconnected(Session session);

        protected override void OnInit()
        {
            IPEndPoint iPEndPoint = null;
            if (Config != null)
            {
                var port = int.Parse(Config.Network.Port.Value);
                iPEndPoint = IPEndPoint.Parse(Config.Network.Host.Value + ":" + port);
            }
            netWorkMngr.Init(iPEndPoint, SyncContext, OnHandlerData, OnHandlerConnected, OnHandlerDisconnected);
        }

        protected override void OnUpdate()
        {

            netWorkMngr.Update();
        }

        protected override void OnFini()
        {

            netWorkMngr.Fini();
        }

    }
}
