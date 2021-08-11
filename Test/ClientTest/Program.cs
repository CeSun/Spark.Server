using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static BlockingCollection<double> bc = new BlockingCollection<double>();
        static TcpClient[] clients;
        static async Task Main(string[] args)
        {
             List<Task> tasks = new List<Task>();
            await Task.Delay(3000);
            int num = 4000;
            clients =  new TcpClient[num];
            for (int i = 0; i < num; i++)
            {
                clients[i] = new TcpClient();
                clients[i].Connect("127.0.0.1", 2007);
            }
            Console.WriteLine("Start!");
            for (int i= 0; i < num; i++)
            {
                // tasks.Add(TestDirServer());
                tasks.Add(fun3(String.Format("openid_xxx|{0}", i), i));
            }
            foreach (var task in tasks)
            {
                await task;
            }
            Console.WriteLine(bc.Average());
        }
        static async Task TestDirServer()
        {
            byte[] readBuffer = new byte[1024 * 1024];
            TcpClient client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 2008);
            Dirapi.SHead head = new Dirapi.SHead();
            head.Msgid = Dirapi.EOpCode.RegisterReq;
            head.Sync = 5555;
            Dirapi.RegisterReq req = new Dirapi.RegisterReq();
            req.Info = new Dirapi.ServerInfo();
            req.Info.Name = "GameServer";
            req.Info.Url = new Dirapi.IpAndPort {Ip="localhost", Port = 2007 };
            req.Info.Zone = 1;
            var sendbyte = pack(head, req);
            var stream = client.GetStream();
            if(true)
            {
                await stream.WriteAsync(sendbyte);
                await stream.ReadAsync(readBuffer);
                Dirapi.RegisterRsp rsp;
                unpack(out head, out rsp, readBuffer);
                Console.WriteLine("register succ");
            }
        }
        static async Task fun()
        {
            byte[] readBuffer = new byte[1024 * 1024];
            TcpClient client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 2007);
            SHead head = new SHead();
            head.Msgid = EOpCode.TestReq;
            head.Reqseq = 0;
            var testReq = new TestReq();
            var reqByte = pack(head, testReq);
            var stream = client.GetStream();
            await stream.WriteAsync(reqByte);
            await stream.ReadAsync(readBuffer);
            TestRsp rsp;
            unpack(out head, out rsp, readBuffer);
        }
        static async Task<double> fun(string name, int index)
        {
            byte[] readBuffer = new byte[1024 * 1024];
            var client = clients[index];
            SHead head = new SHead();
            head.Msgid = EOpCode.LoginReq;
            head.Reqseq = 0;

            TestReq loginReq = new TestReq();
            loginReq.Id = 1;
            loginReq.Name = name;
            var reqByte = pack(head, loginReq);
            var stream = client.GetStream();
            Stopwatch sw = Stopwatch.StartNew();
            await stream.WriteAsync(reqByte);
            await stream.ReadAsync(readBuffer);
            TestRsp loginRsp;
            unpack(out head, out loginRsp, readBuffer);
            sw.Stop();
            bc.Add(sw.Elapsed.TotalMilliseconds);
            client.Close();
            return sw.Elapsed.TotalMilliseconds;


        }
        static async Task fun3(string name, int index)
        {
            byte[] readBuffer = new byte[1024 * 1024];
            var client = clients[index];
            SHead head = new SHead();
            head.Msgid = EOpCode.LoginReq;
            head.Reqseq = 0;

            LoginReq loginReq = new LoginReq();
            loginReq.LoginType = ELoginType.TestLogin;
            loginReq.TestAccount = name;
            var reqByte = pack(head, loginReq);
            var stream = client.GetStream();
            await stream.WriteAsync(reqByte);
            await stream.ReadAsync(readBuffer);
            LoginRsp loginRsp;
            unpack(out head, out loginRsp, readBuffer);
            head.Msgid = EOpCode.CreateroleReq;
            var createRole = new CreateRoleReq { NickName = name };
            reqByte = pack(head, createRole);
            await stream.WriteAsync(reqByte);
            await stream.ReadAsync(readBuffer);
            CreateRoleRsp createRoleRsp;
            unpack(out head, out createRoleRsp, readBuffer);

            if (head.Errcode == EErrno.Succ)
            {
                Console.WriteLine("玩家：" + createRoleRsp.PlayerInfo.NickName + ", 角色创建成功！");
            }
            else
            {
                Console.WriteLine("玩家：" + ", 创建失败！错误码:" + head.Errcode);
            }

            while (true)
            {
                head.Msgid = EOpCode.TestReq;
                var heartbeatReq = new TestReq();
                reqByte = pack(head, heartbeatReq);
                await stream.WriteAsync(reqByte);
                await stream.ReadAsync(readBuffer);
                TestRsp heartBeatRsp;
                unpack(out head, out heartBeatRsp, readBuffer);
                await Task.Delay(3000);
                Console.WriteLine("已收到应答包！");
            }


        }
        static void unpack<THead, TBody>(out THead head, out TBody body, byte[] data) where TBody : IMessage<TBody>, new() where THead: IMessage<THead>, new()
        {
            var packlenBits = data.Skip(0).Take(sizeof(int)).ToArray();
            Array.Reverse(packlenBits);
            var packlen = BitConverter.ToInt32(packlenBits, 0);

            var headlenBits = data.Skip(sizeof(int)).Take(sizeof(int)).ToArray();
            Array.Reverse(headlenBits);
            var headlen = BitConverter.ToInt32(headlenBits, 0);
            MessageParser<THead> header = new MessageParser<THead>(() => new THead());
            head = header.ParseFrom(data, sizeof(int) * 2, headlen);

            MessageParser<TBody> parser = new MessageParser<TBody>(() => new TBody());
            body = parser.ParseFrom(data, sizeof(int) * 2 + headlen, packlen - (sizeof(int) * 2 + headlen));

        }
        static byte[] pack(IMessage head, IMessage body)
        {
            byte[] headbytes = null;
            byte[] bodybytes = null;
            // 将head序列化
            using (var memory = new MemoryStream())
            {
                headbytes = head.ToByteArray();
            }
            // 将body序列化
            using (var memory = new MemoryStream())
            {
                bodybytes = body.ToByteArray();
            }
            if (headbytes == null || bodybytes == null) { return null; }
            List<byte> list = new List<byte>();

            // 算出包总长度，并序列化为byte[] 大端序
            Int32 len = headbytes.Length + bodybytes.Length + 8;
            List<byte> lenlist = new List<byte>(BitConverter.GetBytes(len));
            lenlist.Reverse();


            // 算出head长度，并序列化为byte[] 大端序
            Int32 headlen = headbytes.Length;
            List<byte> headlenlist = new List<byte>(BitConverter.GetBytes(headlen));
            headlenlist.Reverse();

            // 依次加入总长度, head长度, head序列化, body序列化 
            list.AddRange(lenlist);
            list.AddRange(headlenlist);
            list.AddRange(headbytes);
            list.AddRange(bodybytes);
            return list.ToArray();
        }
    }
}
