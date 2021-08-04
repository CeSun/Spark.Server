using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public abstract class ServerAppWithNet<SubT> : ServerApp<SubT> where SubT : ServerApp<SubT>, new()
    {
        NetworkMngr netWorkMngr = new NetworkMngr();
        /// <summary>
        /// tcp协议处理
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="data">二进制数据</param>
        /// <returns>无，可以await</returns>
        protected abstract Task OnHandlerData(Session session, byte[] data);
        protected abstract void OnConnect(Session session);
        protected abstract void OnDisconnect(Session session);


        protected override void OnInit()
        {
            IPEndPoint iPEndPoint = null;
            if (Config != null)
            {
                var port = int.Parse(Config.Network.Port.Value);
                iPEndPoint = IPEndPoint.Parse(Config.Network.Host.Value + ":" + port);
            }
            netWorkMngr.Init(iPEndPoint, OnHandlerData, OnConnect, OnDisconnect);
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
