﻿using Protocol;
using Google.Protobuf;
using System;
using System.Threading.Tasks;
using Frame;
using System.Security.Principal;
using System.Diagnostics;
using CacheServerApi;
using CacheServerApi.Tables;
using ProxyServerApi.Tables;

namespace GameServer.Player
{
    public class Player
    {
        public Frame.Session Session { get; private set; }
        DispatcherLite<EOpCode, SHead, EErrno> dispatcher;
        FSM<EEvent, EState> fsm = new FSM<EEvent, EState>();    
        uint LatestSeq = 0;
        DateTime LatestTime;

        // 数据库数据的pb
        public PBPlayer DBData 
        {
            get
            {
                if (tPlayer != null)
                    return tPlayer.Base;
                else
                    return null;
            }
        }

        // 数据库数据的orm, 对外修改等使用上面的
        private TPlayer tPlayer;

        public PBAccount DBAccount { get; private set; }

        public void Disconnected()
        {
            fsm.PostEvent(EEvent.Logout);
        }

        public async Task processData(byte[] data)
        {
          await dispatcher.DispatcherRequest(data);
          
        }

        public Player(Session session)
        {
            Session = session;
            dispatcher = new DispatcherLite<EOpCode, SHead, EErrno>(
                new DispatcherLite<EOpCode, SHead, EErrno>.Config
                {
                    FunGetMsgId = head => head.Msgid,
                    FunInitHead = (ref SHead rspHead, SHead ReqHead, EOpCode msgId, EErrno err) => {
                        rspHead.Msgid = msgId;
                        rspHead.Errcode = err;
                    },
                    ExceptionErrCode = EErrno.Error,
                    FunSendToClient = SendToClientAsync,
                } );
        }

        public void Init()
        {
            LatestTime = DateTime.Now;
            dispatcher.Bind<TestReq, TestRsp>(EOpCode.TestReq, EOpCode.TestRsp, HelloHandler);
            dispatcher.Bind<LoginReq, LoginRsp>(EOpCode.LoginReq, EOpCode.LoginRsp, LoginAsync);
            dispatcher.Bind<CreateRoleReq, CreateRoleRsp>(EOpCode.CreateroleReq,EOpCode.CreateroleRsp, CreateRoleAsync);
            dispatcher.Bind<LogoutReq, LogoutRsp>(EOpCode.LogoutReq, EOpCode.LogoutRsp, Logout);
            // 给个null函数没关系，这个协议会被filter拦截
            dispatcher.Bind<HeartBeatReq, HeartBeatRsp>(EOpCode.HeartbeatReq, EOpCode.HeartbeatRsp, null);
            dispatcher.Filter = filterAsync;
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
            var ret = fsm.AddEvent(EEvent.Login, EState.Init, EState.Logining, null);
            ret = fsm.AddEvent(EEvent.Create, EState.Logining, EState.Creating, null);
            ret = fsm.AddEvent(EEvent.LoginSucc, EState.Logining, EState.Online, null);
            ret = fsm.AddEvent(EEvent.LoginSucc, EState.Creating, EState.Online, null);
            ret = fsm.AddEvent(EEvent.Logout, EState.Online, EState.LogOut, null);
            ret = fsm.AddEvent(EEvent.Logout, EState.Init, EState.LogOut, null);
            ret = fsm.AddEvent(EEvent.Logout, EState.Creating, EState.LogOut, null);
            ret = fsm.AddEvent(EEvent.Logout, EState.Logining, EState.LogOut, null);
            fsm.Start(EState.Init);

        }
        async Task<(SHead, LogoutRsp)> Logout(SHead head, LogoutReq reqBody)
        {
            // 登出
            fsm.PostEvent(EEvent.Logout);
            return (new SHead {Msgid=EOpCode.LogoutRsp, Errcode = EErrno.Succ }, new LogoutRsp { });
        }
        void OnLogOut()
        {
            // 登出时保存数据
            if (tPlayer != null )
            {
                DBData.LoginServerId = 0;
                _ = tPlayer.SaveAync();
            }
        }

        async Task<(SHead, IMessage)> filterAsync(SHead reqHead,  TaskAction<SHead> next)
        {
            LatestTime = DateTime.Now;
            if (LatestSeq == 0 && reqHead.Msgid != EOpCode.LoginReq)
            {
                fsm.PostEvent(EEvent.Logout);
                return default;
            }
            else if (reqHead.Msgid == EOpCode.HeartbeatReq)
            {
                SHead head = new SHead();
                head.Msgid = EOpCode.HeartbeatRsp;
                HeartBeatRsp heartBeatRsp = new HeartBeatRsp();
                return (head, heartBeatRsp);
            }
            return await next();
        }
        async Task<(SHead, LoginRsp)> LoginAsync(SHead reqHead, LoginReq loginReq)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.LoginRsp, Errcode = EErrno.Succ };
            LoginRsp rspBody = new LoginRsp() {LoginResult = ELoginResult.Success };
            fsm.PostEvent(EEvent.Login);
            var retval = await TAccount.QueryAync(((AuthType)loginReq.LoginType, loginReq.TestAccount, Server.Instance.Zone));
            if (retval.Error == DBError.Success)
            {
                var playerPair = await TPlayer.QueryAync((retval.Row.Base.Zone, retval.Row.Base.Uin));
                if (playerPair.Error == DBError.Success)
                {
                    tPlayer = playerPair.Row;
                    tPlayer.Base.LastLoginTime = DateTime.Now.Millisecond;
                    tPlayer.Base.LoginServerId = Server.Instance.InstanceId;
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
                        DBAccount = retval.Row.Base;
                        fsm.PostEvent(EEvent.Logout);
                        rspHead.Errcode = EErrno.Error;
                    }
                }
                else
                {
                    DBAccount = retval.Row.Base;
                    fsm.PostEvent(EEvent.Create);
                    rspBody.LoginResult = ELoginResult.NoPlayer;
                }
            }
            else
            {
                var uin = await Server.Instance.UinMngr.GetUinAsync();
                if (uin == 0)
                {
                    fsm.PostEvent(EEvent.Logout);
                    rspHead.Errcode = EErrno.Error;
                }
                else
                {
                    var account = TAccount.New();
                    account.Base.Uin = uin;
                    account.Base.Zone = Server.Instance.Zone;
                    account.Base.Type = (AuthType)loginReq.LoginType;
                    account.Base.Account = loginReq.TestAccount;
                    var ret = await account.SaveAync();
                    if (ret == DBError.Success)
                    {
                        DBAccount = account.Base;
                        fsm.PostEvent(EEvent.Create);
                        rspBody.LoginResult = ELoginResult.NoPlayer;
                    }
                    else
                    {
                        fsm.PostEvent(EEvent.Logout);
                        rspHead.Errcode = EErrno.Error;
                    }
                }
            }
            return (rspHead, rspBody);
        }

        async Task<(SHead, IMessage)> LoginFake(SHead reqHead, LoginReq reqBody)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.TestRsp, Errcode = EErrno.Succ };
            TestRsp rspBody = new TestRsp { Id = 1, Name = "Test" };
            var retval = await TAccount.QueryAync((AuthType.Test, "112", Server.Instance.Zone));
            return (rspHead, rspBody);
        }
        async Task<(SHead, TestRsp)> HelloHandler( SHead reqHead, TestReq reqBody)
        {
            SHead rspHead = new SHead {Msgid= EOpCode.TestRsp, Errcode = EErrno.Succ };
            TestRsp rspBody = new TestRsp { Id = 1, Name = "Test" };
            return (rspHead, rspBody);
        }

        async Task<(SHead, CreateRoleRsp)> CreateRoleAsync(SHead reqHead, CreateRoleReq reqBody)
        {
            SHead rspHead = new SHead { Msgid = EOpCode.CreateroleRsp, Errcode = EErrno.Succ };
            CreateRoleRsp rspBody = new CreateRoleRsp { };
            if (fsm.CurrentState == EState.Creating)
            {
                var nkName = TNickname.New();
                nkName.Base.Nickname = reqBody.NickName;
                nkName.Base.Uin = DBAccount.Uin;
                nkName.Base.Zone = Server.Instance.Zone;
                var ret = await nkName.SaveAync();
                if (ret == DBError.Success)
                {
                    tPlayer = TPlayer.New();
                    tPlayer.Base.Uin = DBAccount.Uin;
                    tPlayer.Base.Zone = Server.Instance.Zone;
                    tPlayer.Base.LastLoginTime = DateTime.Now.Millisecond;
                    tPlayer.Base.LoginServerId = Server.Instance.InstanceId;
                    tPlayer.Base.Nickname = reqBody.NickName;

                    rspBody.PlayerInfo = new PlayerInfo();
                    fillPlayerInfo(rspBody.PlayerInfo);
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
            return (rspHead, rspBody);
        }

        void fillPlayerInfo (PlayerInfo playerInfo)
        {
            playerInfo.Uin = DBData.Uin;
            playerInfo.NickName = DBData.Nickname;
        }
        public async Task SendToClientAsync<TRsp>(SHead head, TRsp rsp) where TRsp : IMessage
        {
            head.Reqseq = ++LatestSeq;
            byte[] data = null;
            await Task.Run(() =>
            {
                var bodyBits = rsp.ToByteArray();
                var headBits = head.ToByteArray();
                int packLength = bodyBits.Length + headBits.Length + 2 * sizeof(int);
                data = new byte[packLength];
                var packLengthBits = BitConverter.GetBytes(packLength);
                var headLengthBits = BitConverter.GetBytes(headBits.Length);
                Array.Reverse(packLengthBits);
                Array.Reverse(headLengthBits);
                packLengthBits.CopyTo(data, 0);
                headLengthBits.CopyTo(data, sizeof(int));
                headBits.CopyTo(data, 2 * sizeof(int));
                bodyBits.CopyTo(data, 2 * sizeof(int) + headBits.Length);
            });
            await Session.SendAsync(data);
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
