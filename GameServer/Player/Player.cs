using Protocol;
using Google.Protobuf;
using System;
using System.Threading.Tasks;
using Frame;

namespace GameServer.Player
{
    public class Player
    {
        public Session Session { get; private set; }
        Dispatcher<EOpcode, SHead> dispatcher = new Dispatcher<EOpcode, SHead>(head => { return head.Msgid; });
        FSM<EEvent, EState> fsm = new FSM<EEvent, EState>();    
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
            fsm.AddState(EState.Init, null, null, null);
            fsm.AddState(EState.Logining, null, null, null);
            fsm.AddState(EState.LogOut, null, null, null);
            fsm.AddState(EState.Online, null, null, null);
            fsm.AddState(EState.LogOut, null, null, null);

            fsm.AddEvent(EEvent.Login, EState.Init, EState.Logining, null);
            fsm.AddEvent(EEvent.Create, EState.Logining, EState.Creating, null);

            fsm.AddEvent(EEvent.LoginSucc, EState.Logining, EState.Online, null);
            fsm.AddEvent(EEvent.LoginSucc, EState.Creating, EState.Online, null);


            fsm.AddEvent(EEvent.Logout, EState.Online, EState.LogOut, null);
            fsm.AddEvent(EEvent.Logout, EState.Init, EState.LogOut, null);
            fsm.AddEvent(EEvent.Logout, EState.Creating, EState.LogOut, null);
            fsm.AddEvent(EEvent.Logout, EState.Logining, EState.LogOut, null);

            fsm.Start(EState.Init);
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
            Array.Reverse(packLengthBits);
            Array.Reverse(headLengthBits);
            packLengthBits.CopyTo(data, 0);
            headLengthBits.CopyTo(data, sizeof(int));
            headBits.CopyTo(data, 2 * sizeof(int));
            bodyBits.CopyTo(data, 2 * sizeof(int) + headBits.Length);
            Session.Send(data);
        }

        public void Update()
        {
            fsm.Update();
        }
        public void Fini()
        {

        }
    }

    enum EState
    {
        Init,
        Logining,
        Creating,
        Online,
        LogOut
    }

    enum EEvent
    {
        Login,
        LoginSucc,
        Create,
        Logout,
        DbError
    }
}
