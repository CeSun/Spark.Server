using Protocol;
using Google.Protobuf;
using System;
using System.Threading.Tasks;
using Frame;
using MySqlX.XDevAPI;
using DataBase.Tables;
using DataBase;

namespace GameServer.Player
{
    public class Player
    {
        public Frame.Session Session { get; private set; }
        Dispatcher<EOpCode, SHead> dispatcher = new Dispatcher<EOpCode, SHead>(head => { return head.Msgid; });
        FSM<EEvent, EState> fsm = new FSM<EEvent, EState>();    
        uint LatestSeq = 0;
        DateTime LatestTime;
        public DBPlayer DBData { get { return tPlayer.Value; } }
        private TPlayer tPlayer;
        public bool IsDisConnected { get; private set; }
        public async Task processData(byte[] data)
        {
            await dispatcher.DispatcherRequest(data);
        }

        public Player(Frame.Session session)
        {
            IsDisConnected = false;
            Session = session;
        }

        public void Init()
        {
            LatestTime = DateTime.Now;
            dispatcher.Bind<TestReq>(EOpCode.TestReq, HelloHandler);
            dispatcher.Bind<LoginReq>(EOpCode.LoginReq, LoginAsync);
            dispatcher.requestHandlers.Add(RequestHandler);
            InitFSM();
        }

        private void InitFSM()
        {

            fsm.AddState(EState.Init, null, null, null);
            fsm.AddState(EState.Logining, null, null, null);
            fsm.AddState(EState.LogOut, null, null, null);
            fsm.AddState(EState.Online, null, null, null);
            fsm.AddState(EState.LogOut, OnLogOut, null, null);

            // 每个事件对应的处理函数还没有捋一遍
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

        void OnLogOut()
        {
            // 登出时保存数据
            DBData.LoginServerId = 0;
            _ = tPlayer.SaveAync();
        }
        bool RequestHandler(SHead reqHead)
        {
            LatestTime = DateTime.Now;
            if (LatestSeq == 0)
            {
                if (reqHead.Msgid != EOpCode.LoginReq)
                {
                    fsm.PostEvent(EEvent.Logout);
                    return false;
                }
            }
            if (reqHead.Reqseq == LatestSeq)
            {
                return true;
            }
            if (reqHead.Msgid != EOpCode.HeartbeatReq)
            {
                fsm.PostEvent(EEvent.Logout);
            }

            return false;
        }

        async Task LoginAsync(SHead reqHead, LoginReq loginReq)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.LoginRsp, Errcode = EErrno.Succ };
            LoginRsp rspBody = new LoginRsp();
            fsm.PostEvent(EEvent.Login);
            var retval = await TAccount.QueryAync(((DataBase.AuthType)loginReq.LoginType, loginReq.TestAccount, Server.Instance.Zone));
            if (retval.Error == DataBase.DBError.Success)
            {
                var playerPair = await TPlayer.QueryAync((retval.Row.Value.Zone, retval.Row.Value.Uin));
                if (playerPair.Error == DataBase.DBError.Success)
                {
                    tPlayer = playerPair.Row;
                    tPlayer.Value.LastLoginTime = DateTime.Now.Millisecond;
                    tPlayer.Value.LoginServerId = Server.Instance.InstanceId;
                    var ret = await tPlayer.SaveAync();
                    if (ret == DBError.Success)
                    {
                        fsm.PostEvent(EEvent.LoginSucc);
                        rspBody.PlayerInfo = new PlayerInfo();
                        rspBody.PlayerInfo.Uin = DBData.Uin;
                        rspBody.PlayerInfo.NickName = DBData.Nickname;
                        rspBody.LoginResult = ELoginResult.Success;
                    }
                    else
                    {
                        fsm.PostEvent(EEvent.Logout);
                        rspHead.Errcode = EErrno.Error;
                    }
                }
                else
                {
                    fsm.PostEvent(EEvent.Create);
                    rspBody.LoginResult = ELoginResult.NoPlayer;
                }
            }
            else
            {
                var uin = await Server.Instance.UinMngr.GetUinAsync();
                var account = TAccount.New();
                account.Value.Uin = uin;
                account.Value.Type = (DataBase.AuthType)loginReq.LoginType;
                account.Value.Account = loginReq.TestAccount;
                await account.SaveAync();
                fsm.PostEvent(EEvent.Create);
                rspBody.LoginResult = ELoginResult.NoPlayer;
            }
            await SendToClientAsync(rspHead, rspBody);
        }

        async Task HelloHandler( SHead reqHead, TestReq reqBody)
        {
            SHead rspHead = new SHead {Msgid= EOpCode.TestRsp, Errcode = EErrno.Succ };
            TestRsp rspBody = new TestRsp { Id = 1, Name = "Test" };
            await Task.Delay(0);
            await SendToClientAsync(rspHead, rspBody);
        }

        public async Task SendToClientAsync<TRsp>(SHead head, TRsp rsp) where TRsp : IMessage
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
            await Session.SendAsync(data);
        }

        public void Update()
        {
            if (fsm.CurrentState != EState.Init && fsm.CurrentState != EState.LogOut)
            {
                if ((DateTime.Now - LatestTime).TotalSeconds > 5)
                {
                    fsm.PostEvent(EEvent.Logout);
                }
            }
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
