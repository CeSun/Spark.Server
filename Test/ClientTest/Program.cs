﻿using Google.Protobuf;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace ClientTest
{
    class Program
    {
        static byte[] readBuffer = new byte[1024 * 1024];
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 2007);
            SHead head = new SHead();
            head.Msgid = EOpCode.LoginReq;
            head.Reqseq = 0;
            LoginReq loginReq = new LoginReq();
            loginReq.LoginType = ELoginType.TestLogin;
            loginReq.TestAccount = "3213";
            var reqByte = pack(head, loginReq);
            var stream = client.GetStream();
            stream.Write(reqByte);
            stream.Read(readBuffer);
            LoginRsp loginRsp;
            unpack(out head, out loginRsp, readBuffer);
            
            if (loginRsp.LoginResult == ELoginResult.NoPlayer)
            {
                head.Msgid = EOpCode.CreateroleReq;
                CreateRoleReq createrole = new CreateRoleReq() { NickName = "测试名称" };
                reqByte = pack(head, createrole);

                stream.Write(reqByte);
                stream.Read(readBuffer);
                CreateRoleRsp createRoleRsp;
                unpack(out head, out loginRsp, readBuffer);
                if (head.Errcode == EErrno.Succ)
                {
                   
                }
                else
                {

                }

            } else
            {

            }



        }

        static void unpack<TBody>(out Protocol.SHead head, out TBody body, byte[] data) where TBody : IMessage<TBody>, new()
        {
            var packlenBits = data.Skip(0).Take(sizeof(int)).ToArray();
            Array.Reverse(packlenBits);
            var packlen = BitConverter.ToInt32(packlenBits, 0);

            var headlenBits = data.Skip(sizeof(int)).Take(sizeof(int)).ToArray();
            Array.Reverse(headlenBits);
            var headlen = BitConverter.ToInt32(headlenBits, 0);
            head = Protocol.SHead.Parser.ParseFrom(data, sizeof(int) * 2, headlen);

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