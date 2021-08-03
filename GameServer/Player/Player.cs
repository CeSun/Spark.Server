using Protocol;
using Google.Protobuf;
using System;
using System.Threading.Tasks;
using Frame;
using MySqlX.XDevAPI;
using DataBase.Tables;
using DataBase;
using System.Security.Principal;

namespace GameServer.Player
{
    public class Player
    {
        public Frame.Session Session { get; private set; }
        Dispatcher<EOpCode, SHead> dispatcher = new Dispatcher<EOpCode, SHead>(head => { return head.Msgid; });
        FSM<EEvent, EState> fsm = new FSM<EEvent, EState>();    
        uint LatestSeq = 0;
        DateTime LatestTime;

        // 数据库数据的pb
        public DBPlayer DBData { get { return tPlayer.Value; } }

        // 数据库数据的orm, 对外修改等使用上面的
        private TPlayer tPlayer;

        public DBAccount DBAccount { get; private set; }


        public bool IsDisConnected { get; private set; }
        public async Task processData(byte[] data)
        {
            await dispatcher.DispatcherRequest(data);
            // 每次请求结束 保存数据
            if (tPlayer != null)
            {
                await tPlayer.SaveAync();
            }
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
            dispatcher.Bind<CreateRoleReq>(EOpCode.CreateroleReq, CreateRoleAsync);
            dispatcher.requestHandlers.Add(RequestHandler);
            InitFSM();
        }

        private void InitFSM()
        {

            fsm.AddState(EState.Init, null, null, null);
            fsm.AddState(EState.Logining, null, null, null);
            fsm.AddState(EState.LogOut, OnLogOut, null, null);
            fsm.AddState(EState.Online, null, null, null);
            fsm.AddState(EState.Creating, null, null, null);

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
            LoginRsp rspBody = new LoginRsp() {LoginResult = ELoginResult.Success };
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
                        fillPlayerInfo(rspBody.PlayerInfo);
                        rspBody.LoginResult = ELoginResult.Success;
                    }
                    else
                    {
                        DBAccount = retval.Row.Value;
                        fsm.PostEvent(EEvent.Logout);
                        rspHead.Errcode = EErrno.Error;
                    }
                }
                else
                {

                    DBAccount = retval.Row.Value;
                    fsm.PostEvent(EEvent.Create);
                    rspBody.LoginResult = ELoginResult.NoPlayer;
                }
            }
            else
            {
                var uin = await Server.Instance.UinMngr.GetUinAsync();
                var account = TAccount.New();
                account.Value.Uin = uin;
                account.Value.Zone = Server.Instance.Zone;
                account.Value.Type = (DataBase.AuthType)loginReq.LoginType;
                account.Value.Account = loginReq.TestAccount;
                var ret = await account.SaveAync();
                if (ret == DBError.Success)
                {
                    DBAccount = account.Value;
                    fsm.PostEvent(EEvent.Create);
                    rspBody.LoginResult = ELoginResult.NoPlayer;
                } else
                {
                    fsm.PostEvent(EEvent.Logout);
                    rspHead.Errcode = EErrno.Error;
                }
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

        async Task CreateRoleAsync(SHead reqHead, CreateRoleReq reqBody)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.CreateroleRsp, Errcode = EErrno.Succ };
            CreateRoleRsp rspBody = new CreateRoleRsp { };
            if (fsm.CurrentState == EState.Creating)
            {
                var nkName = TNickname.New();
                nkName.Value.Nickname = reqBody.NickName;
                nkName.Value.Uin = DBAccount.Uin;
                nkName.Value.Zone = Server.Instance.Zone;
                var ret = await nkName.SaveAync();
                if (ret == DBError.Success)
                {
                    tPlayer = TPlayer.New();
                    tPlayer.Value.Uin = DBAccount.Uin;
                    tPlayer.Value.Zone = Server.Instance.Zone;
                    tPlayer.Value.LastLoginTime = DateTime.Now.Millisecond;
                    tPlayer.Value.LoginServerId = Server.Instance.InstanceId;
                    tPlayer.Value.Nickname = reqBody.NickName;
                    fsm.PostEvent(EEvent.LoginSucc);
                }
                else if(ret == DBError.IsExisted)
                {
                    rspHead.Errcode = EErrno.NicknameExisted;
                }
                else
                {
                    rspHead.Errcode = EErrno.Error;
                }
            }
            else if (fsm.CurrentState == EState.Online)
            {
                rspHead.Errcode = EErrno.RoleExisted;
            } else
            {
                rspHead.Errcode = EErrno.Error;
            }
            await SendToClientAsync(rspHead, rspBody);
        }

        void fillPlayerInfo (PlayerInfo playerInfo)
        {
            playerInfo.Uin = DBData.Uin;
            playerInfo.NickName = DBData.Nickname;
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
                if ((DateTime.Now - LatestTime).TotalSeconds > 100)
                {
                    // fsm.PostEvent(EEvent.Logout);
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
