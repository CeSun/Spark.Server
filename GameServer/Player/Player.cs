using Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Player
{
    public class Player
    {
        public Session Session { get; private set; }
        Frame.Dispatcher<EOpcode, SHead> dispatcher = new Frame.Dispatcher<EOpcode, SHead>(head => { return head.Msgid; });
        uint LatestSeq = 0;
        public bool IsDisConnected { get; private set; }
        public async Task processData(byte[] data)
        {
            await dispatcher.DispatcherRequest(data);
        }

        public Player(Session session)
        {
            IsDisConnected = false;
            Session = session;
            dispatcher.Bind<TestReq>(EOpcode.TestReq, HelloHandler);
            dispatcher.requestHandlers.Add(RequestHandler);
        }

        bool RequestHandler(SHead reqHead)
        {
            if (reqHead.Reqseq == LatestSeq)
            {
                return true;
            }
            return false;
        }


        async Task HelloHandler(SHead reqHead, TestReq reqBody)
        {
            SHead rspHead = new SHead {Msgid=EOpcode.TestRsp, Errcode = EErrno.EcserrnoSucc };
            TestRsp resBody = new TestRsp { Id = 1, Name = "Test" };
            await Task.Delay(0);
            SendToClientAsync(rspHead, resBody);
        }

        public void SendToClientAsync<TRsp>(SHead head, TRsp rsp) where TRsp : IMessage
        {
            head.Reqseq = ++LatestSeq;
            var bodyBits = rsp.ToByteArray();
            var headBits = head.ToByteArray();
            int packLength = bodyBits.Length + headBits.Length + 2 * sizeof(int);
            byte[] data = new byte[packLength];
            var packLengthBits = BitConverter.GetBytes(packLength);
            var headLengthBits = BitConverter.GetBytes(headBits.Length);
            packLengthBits.CopyTo(data, 0);
            headLengthBits.CopyTo(data, sizeof(int));
            headBits.CopyTo(data, 2 * sizeof(int));
            bodyBits.CopyTo(data, 2 * sizeof(int) + headBits.Length);
            Session.Send(data);
        }

        public void Update()
        {

        }
        public void Fini()
        {

        }
    }
}
