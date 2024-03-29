﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{

    public delegate Task<(THead, IMessage)> TaskAction<THead>();
    /// <summary>
    /// 协议派发类
    /// </summary>
    /// <typeparam name="TMsgId">消息id类型</typeparam>
    /// <typeparam name="THead">消息头类型</typeparam>
    /// 
    
    public class Dispatcher <TMsgId, THead, TTransparent, TErr> where THead : IMessage<THead>, new()
    {
        public delegate TMsgId GetMsgIdFunc(THead head);
        public delegate Task SendToClient(TTransparent trans, THead head, IMessage body);
        public delegate void InitHeadFunc(ref THead rspHead, THead ReqHead, TMsgId msgId, TErr err);
         public class Config
        {
            public GetMsgIdFunc FunGetMsgId;

            public InitHeadFunc FunInitHead;

            public TErr ExceptionErrCode;

            public SendToClient FunSendToClient;

        }
        Config config;
        MessageParser<THead> HeadParser = new MessageParser<THead>(() => new THead());
        /// <summary>
        /// 协议处理函数
        /// </summary>
        /// <typeparam name="TBody">消息体的类型</typeparam>
        /// <param name="head">消息头</param>
        /// <param name="body">消息体</param>
        /// <returns>无返回值</returns>
        public delegate Task<(THead, TRsp)> ProcessFunWithSession<TReq, TRsp>(TTransparent session, THead head, TReq body)where TReq: IMessage<TReq> where TRsp : IMessage<TRsp>;

        /// <summary>
        /// 从Head中取消息id函数
        /// </summary>
        /// <param name="head">head对象</param>
        /// <returns>返回消息id</returns>

        public delegate Task<(THead, IMessage)> RequestHandler(TTransparent transparent, THead head, TaskAction<THead> next,int offset, byte[] data);

        private RequestHandler requestHandler = async (transparent, head, next, offset, data) => await next();

        public RequestHandler Filter { get => requestHandler; set {
                if (value != null) requestHandler = value; 
            } }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="func">需要一个函数，从head中取msgid</param>
        public Dispatcher(Config config)
        {
            this.config = config;
            // getMsgId = func;
        }

        /// <summary>
        /// 注册协议处理函数
        /// </summary>
        /// <typeparam name="TBody">消息体类型</typeparam>
        /// <param name="MessageId">消息id</param>
        /// <param name="func">处理函数</param>
        public void Bind<TReq, TRsp>(TMsgId ReqMessageId, TMsgId RspMessageId, ProcessFunWithSession<TReq, TRsp> func) where TReq : IMessage<TReq>, new() where TRsp : IMessage<TRsp>, new()
        {
            Functions.Add(ReqMessageId, async (session, offset, head, data) => {
                (THead head, TRsp body) rsp = default;
                try
                {
                    MessageParser<TReq> parser = new MessageParser<TReq>(() => new TReq());
                    var reqbody = parser.ParseFrom(data, offset, data.Length - offset);
                    if (reqbody == null)
                        rsp = default;
                    rsp = await func(session, head, reqbody);
                } catch (ResponseException<TErr> e)
                {
                    Console.WriteLine();
                    var rspHead = new THead() {};
                    config.FunInitHead(ref rspHead, head, RspMessageId, e.errorcode);
                    rsp = (rspHead, new TRsp());
                }catch
                {
                    var rspHead = new THead() { };
                    config.FunInitHead(ref rspHead, head, RspMessageId, config.ExceptionErrCode);
                    rsp =  (rspHead, new TRsp());
                }
                return rsp;
                
            });
        }

        /// <summary>
        /// 派发协议
        /// </summary>
        /// <param name="data">数据二进制</param>
        /// <returns></returns>
        public async Task DispatcherRequest(TTransparent session, byte[] data)
        {
            var headBits = data.Skip(sizeof(int)).Take(sizeof(int)).ToArray();
            Array.Reverse(headBits);
            var headLength = BitConverter.ToInt32(headBits, 0);
            var head = HeadParser.ParseFrom(data, sizeof(int) *2, headLength);
            if (head == null)
            {
                Console.WriteLine("head parse failed" );
                return;
            }
            var id = config.FunGetMsgId(head);
            var fun = Functions.GetValueOrDefault(id);
            if (fun == null)
            {
                Console.WriteLine($"msg id is not found: {id}");
                return;
            }
            var rsp = await requestHandler(session, head, async () => await fun(session, sizeof(int) * 2 + headLength, head, data), sizeof(int) * 2 + headLength, data);
            if (rsp != (null, null))
            {
                await config.FunSendToClient(session, rsp.Item1, rsp.Item2);
            }
        }


        delegate Task<(THead, IMessage)> ProcessFun(TTransparent session, int offset, THead head, byte[] body);
       
        Dictionary<TMsgId, ProcessFun> Functions = new Dictionary<TMsgId, ProcessFun>();
    }
    
    public class DispatcherLite<TMsgId, THead, TErr> where THead : IMessage<THead>, new()
    {
        Dispatcher<TMsgId, THead, byte, TErr> sub;
        public delegate Task<(THead, TRsp)> ProcessFun<TReq, TRsp>(THead head, TReq body) where TRsp : IMessage<TRsp> where TReq : IMessage<TReq>;

        public delegate Task SendToClient(THead head, IMessage body);
        public class Config : Dispatcher<TMsgId, THead, byte, TErr>.Config
        {
            public new SendToClient FunSendToClient;
        }
        public DispatcherLite(Config config) 
        {
            Dispatcher<TMsgId, THead, byte, TErr>.Config conf = config;
            conf.FunSendToClient = async (session, head, body) =>
            {
                await config.FunSendToClient(head, body);
            };
            sub = new Dispatcher<TMsgId, THead, byte, TErr>(conf);
        }

        public delegate Task<(THead, IMessage)> RequestHandler(THead head, TaskAction<THead> next);
        public async Task DispatcherRequest(byte[] data)
        {
            await sub.DispatcherRequest(default, data);
        }
        public void Bind<TReq, TRsp>(TMsgId MessageId, TMsgId RspMessageId, ProcessFun<TReq, TRsp> func) where TReq : IMessage<TReq>, new() where TRsp : IMessage<TRsp>, new()
        {
            Dispatcher<TMsgId, THead, byte, TErr>.ProcessFunWithSession<TReq, TRsp> newfunc =  async (_, head, body) => await func(head, body);
            sub.Bind(MessageId, RspMessageId, newfunc);
        }
        RequestHandler handler;
        public RequestHandler Filter
        {
            get => handler;
            set  {
                handler = value;
                sub.Filter = async (session, head, next, offset, data) => await handler(head, next);
            }
        }
    }
}
