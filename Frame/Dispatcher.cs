using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    
    public delegate Task TaskAction();
    /// <summary>
    /// 协议派发类
    /// </summary>
    /// <typeparam name="TMsgId">消息id类型</typeparam>
    /// <typeparam name="THead">消息头类型</typeparam>
    public class Dispatcher <TMsgId, THead, TTransparent> where THead : IMessage<THead>, new()
    {
        /// <summary>
        /// 协议处理函数
        /// </summary>
        /// <typeparam name="TBody">消息体的类型</typeparam>
        /// <param name="head">消息头</param>
        /// <param name="body">消息体</param>
        /// <returns>无返回值</returns>
        public delegate Task ProcessFunWithSession<TBody>(TTransparent session, THead head, TBody body);
        public delegate Task ProcessFun<TBody>( THead head, TBody body);

        /// <summary>
        /// 从Head中取消息id函数
        /// </summary>
        /// <param name="head">head对象</param>
        /// <returns>返回消息id</returns>
        public delegate TMsgId GetMsgIdFunc(THead head);

        public delegate Task RequestHandler(THead head, TaskAction next,int offset, byte[] data);

        private RequestHandler requestHandler = async (head, next, offset, data) => await next();

        public RequestHandler Filter { get => requestHandler; set {
                if (value != null) requestHandler = value; 
            } }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="func">需要一个函数，从head中取msgid</param>
        public Dispatcher(GetMsgIdFunc func)
        {
            getMsgId = func;
        }


        /// <summary>
        /// 注册协议处理函数
        /// </summary>
        /// <typeparam name="TBody">消息体类型</typeparam>
        /// <param name="MessageId">消息id</param>
        /// <param name="func">处理函数</param>
        public void Bind<TBody>(TMsgId MessageId, ProcessFunWithSession<TBody> func) where TBody : IMessage<TBody>, new()
        {
            Functions.Add(MessageId, async (session, offset, head, data) => {
                MessageParser<TBody> parser = new MessageParser<TBody>(() => new TBody());
                var rsp = parser.ParseFrom(data, offset, data.Length - offset);
                if (rsp == null)
                    return;
                await func(session, head, rsp);
            });
        }

        /// <summary>
        /// 派发协议
        /// </summary>
        /// <param name="data">数据二进制</param>
        /// <returns></returns>
        public void DispatcherRequest(TTransparent session, byte[] data)
        {
            var headBits = data.Skip(sizeof(int)).Take(sizeof(int)).ToArray();
            Array.Reverse(headBits);
            var headLength = BitConverter.ToInt32(headBits, 0);
            MessageParser<THead> parser = new MessageParser<THead>(() => new THead());
            var head = parser.ParseFrom(data, sizeof(int) *2, headLength);
            if (head == null)
                return;
            if (getMsgId == default)
                return;
            var id = getMsgId(head);
            var fun = Functions.GetValueOrDefault(id);
            if (fun != null)
            {
               _ = requestHandler(head, async () => await fun(session, sizeof(int) * 2 + headLength, head, data), sizeof(int) * 2 + headLength, data);
            }
        }


        delegate Task ProcessFun(TTransparent session, int offset, THead head, byte[] body);
        GetMsgIdFunc getMsgId;
        Dictionary<TMsgId, ProcessFun> Functions = new Dictionary<TMsgId, ProcessFun>();
    }
    
    public class DispatcherLite<TMsgId, THead> where THead : IMessage<THead>, new()
    {
        Dispatcher<TMsgId, THead, byte> sub;
        public DispatcherLite(Dispatcher<TMsgId, THead, byte>.GetMsgIdFunc func) 
        {
            sub = new Dispatcher<TMsgId, THead, byte>(func);
        }

        public void DispatcherRequest(byte[] data)
        {
            sub.DispatcherRequest(default, data);
        }
        public void Bind<TBody>(TMsgId MessageId, Dispatcher<TMsgId, THead, byte>.ProcessFun<TBody> func) where TBody : IMessage<TBody>, new()
        {
            Dispatcher<TMsgId, THead, byte>.ProcessFunWithSession<TBody> newfunc =  async (_, head, body) => await func(head, body);
            sub.Bind(MessageId, newfunc);
        }

        public Dispatcher<TMsgId, THead, byte>.RequestHandler Filter
        {
            get => sub.Filter;
            set => sub.Filter = value;
        }
    }
}
