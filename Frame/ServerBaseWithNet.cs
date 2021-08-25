using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public class BaseNetConfig: BaseConfig
    {
        public NetConfig Network;
        public struct NetConfig
        {
            public string Host;
            public int Port;
        }
    }
    public abstract class ServerBaseWithNet<SubT, TConf> : ServerBase<SubT, TConf> where SubT : ServerBase<SubT, TConf>, new() where TConf : BaseNetConfig
    {
        NetworkMngr netWorkMngr = new NetworkMngr();
        /// <summary>
        /// tcp协议处理
        /// </summary>
        /// <param name="session">session</param>
        /// <param name="data">二进制数据</param>
        /// <returns>无，可以await</returns>
        protected abstract Task OnHandlerData(Session session, byte[] data);
        protected abstract void OnHandlerConnected(Session session);
        protected abstract void OnHandlerDisconnected(Session session);

        protected override void OnInit()
        {
            var iPEndPoint = IPEndPoint.Parse(Config.Network.Host + ":" + Config.Network.Port);
            netWorkMngr.Init(iPEndPoint, OnHandlerData, OnHandlerConnected, OnHandlerDisconnected);
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
