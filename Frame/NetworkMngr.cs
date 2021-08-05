﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frame
{
    /// <summary>
    /// 网络管理类
    /// </summary>
    class NetworkMngr
    {
        Thread recvTask;
        bool Stop;
        TcpListener tcpServer;
        LockFreeQueue<(Session, byte[])> recBufferBlock = new LockFreeQueue<(Session, byte[])>(20000);

        public delegate Task DataHandler(Session session, byte[] data);
        public delegate void ConnectHandler(Session session);
        private DataHandler dataHandler;
        private ConnectHandler connectHandler;
        private ConnectHandler disconnectHandler;
        LockFreeQueue<(Session, bool)> NewSessionBufferBlock = new LockFreeQueue<(Session, bool)>();
        internal LockFreeQueue<(Session, byte[], TaskCompletionSource)> sendBufferBlock = new LockFreeQueue<(Session, byte[], TaskCompletionSource)>();

        IPEndPoint ListenIpEndPoint;
        public void Init(IPEndPoint ListenIpEndPoint, DataHandler dataHandler, ConnectHandler connectHandler, ConnectHandler disconnectHandler)
        {
            this.ListenIpEndPoint = ListenIpEndPoint;
            tcpServer = new TcpListener(ListenIpEndPoint);
            tcpServer.Start();
            recvTask = new Thread(Recv);
            Stop = false;
            recvTask.Start();
            this.dataHandler = dataHandler;
            this.connectHandler = connectHandler;
            this.disconnectHandler = disconnectHandler;
        }

        public void Update()
        {
            List<(Session, byte[])> list;
            recBufferBlock.Get(out list, 10);
            if (dataHandler != null && list != null)
            {
                foreach (var item in list)
                {

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    _ = dataHandler(item.Item1, item.Item2);
                    stopWatch.Stop();
                    Console.WriteLine("once call: " + stopWatch.Elapsed.TotalMilliseconds);

                }
            }
        }

        public void Fini()
        {
            Stop = true;
        }

        Dictionary<ulong, Session> clientMap = new Dictionary<ulong, Session>();
        Dictionary<Socket, ulong> socketMap = new Dictionary<Socket, ulong>();
        Dictionary<Socket, List<(byte[], TaskCompletionSource)>> sendMap = new Dictionary<Socket, List<(byte[], TaskCompletionSource)>>();
        ulong IdIter = 1;
        byte[] dataBuffer = new byte[1024 * 1024 * 5];
        DateTime now;
        void Recv()
        {
            try
            {
                var serverSocket = tcpServer.Server;
                List<Socket> readlist = new List<Socket>();
                List<Socket> writelist = new List<Socket>();
                List<Socket> errorlist = new List<Socket>();
                List<(ulong, Socket)> waitDelete = new List<(ulong, Socket)>();
                while (!Stop)
                {
                    errorlist.Clear();
                    writelist.Clear();
                    waitDelete.Clear();
                    now = DateTime.Now;
                    ResetCheckList(ref readlist);
                    errorlist.AddRange(readlist);
                    Socket.Select(readlist, writelist, errorlist, 1000);
                    List<(Session, byte[], TaskCompletionSource)> outListSend;
                    if (sendBufferBlock.TryGetAll(out outListSend))
                    {
                        foreach (var pair in outListSend)
                        {
                            if (socketMap.GetValueOrDefault(pair.Item1.clientSocket) != 0)
                            {
                                var list = sendMap.GetValueOrDefault(pair.Item1.clientSocket);
                                if (list == null)
                                {
                                    list = new List<(byte[], TaskCompletionSource)>();
                                    sendMap.Add(pair.Item1.clientSocket, list);
                                }
                                list.Add((pair.Item2, pair.Item3));
                            } else
                            {
                                pair.Item3.SetResult();
                                sendMap.Remove(pair.Item1.clientSocket);
                            }
                        }
                    }
                    foreach(var item in sendMap)
                    {
                        writelist.Add(item.Key);
                    }
                    foreach (var item in readlist)
                    {
                        if (item == serverSocket)
                        {
                            var socket = serverSocket.Accept();
                            if (socket != null)
                            {
                                var id = IdIter++;
                                var session = new Session { clientSocket = socket, SessionId = id, networkMngr = this, latestRec = now };
                                clientMap.Add(id, session);
                                socketMap.Add(socket, id);
                                NewSessionBufferBlock.Add((session, true));
                            }
                        }
                        else
                        {
                            var sessionId = socketMap.GetValueOrDefault(item);
                            var len = 0;
                            try
                            {
                                len = item.Receive(dataBuffer);
                            }
                            catch
                            {
                                waitDelete.Add((sessionId, item));
                            }
                            if (len == 0)
                            {
                                waitDelete.Add((sessionId, item));
                            }
                            var id = socketMap.GetValueOrDefault(item);
                            if (id == 0)
                                continue;
                            var session = clientMap.GetValueOrDefault(id);
                            if (session == null)
                                continue;
                            List<byte[]> outlist = new List<byte[]> ();
                            if (session.otherData != null)
                            {
                                len += session.otherData.Length;
                                
                            }
                            
                            var otherData = ReadPackFromBuffer(dataBuffer, session.otherData, len, out outlist);
                            session.otherData = otherData;
                            foreach(var itemdata in outlist)
                            {
                                recBufferBlock.Add((session, itemdata));
                            }
                            session.latestRec = now;
                        }
                    }
                    foreach(var item in writelist)
                    {
                        var sessionId = socketMap.GetValueOrDefault(item);
                        var list  =sendMap.GetValueOrDefault(item);
                        if (list == null)
                            continue;
                        foreach(var data in list)
                        {
                            try
                            {
                                item.Send(data.Item1);
                                data.Item2.SetResult();
                            } catch
                            {
                                waitDelete.Add((sessionId, item));
                                break;
                            }
                        }
                        list.Clear();
                    }
                    foreach(var item in errorlist)
                    {
                        var sessionId = socketMap.GetValueOrDefault(item);
                        waitDelete.Add((sessionId, item));
                    }
                    foreach(var pair in clientMap)
                    {
                        if ((now - pair.Value.latestRec).Seconds >= 10000)
                        {
                            waitDelete.Add((pair.Key, pair.Value.clientSocket));
                        }
                    }
                    waitDelete.ForEach(pair => {
                        pair.Item2.Shutdown(SocketShutdown.Both);
                        var session = clientMap.GetValueOrDefault(pair.Item1);
                        clientMap.Remove(pair.Item1);
                        socketMap.Remove(pair.Item2);
                        var list = sendMap.GetValueOrDefault(pair.Item2);
                        if (list != null)
                            list.ForEach(res => res.Item2.SetResult());
                        sendMap.Remove(pair.Item2);
                        if (session != null)
                            NewSessionBufferBlock.Add((session, false));

                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        void ResetCheckList(ref List<Socket> checklist)
        {
            checklist.Clear();
            checklist.Add(tcpServer.Server);
            foreach(var pair in clientMap)
            {
                checklist.Add(pair.Value.clientSocket);
            }
        }
        byte[] ReadPackFromBuffer(byte[] buffer1, byte[] otherData, int dataLength, out List<byte[]> outList)
        {
            var buffer = new byte[dataLength];
            int otherLen = 0;
            if (otherData != null) otherLen=  otherData.Length;
            if (otherData != null)
                Array.Copy(otherData, buffer, otherData.Length);
            Array.Copy(buffer1, 0, buffer, otherLen, buffer.Length - otherLen);
            outList = new List<byte[]>();
            int start = 0;
            int length = 0;
            byte[] temp;
            do
            {
                if (start >= dataLength)
                    return null;
                if (length != 0)
                {
                    if (start + length > buffer.Length)
                    {
                        temp = buffer.Skip(start).Take((buffer.Length - start)).ToArray();
                        return temp;
                    }
                    var data = buffer.Skip(start).Take(length).ToArray();
                    outList.Add(data);
                    start = start + length;
                }
                if (start + sizeof(int) > buffer.Length)
                {
                    temp = buffer.Skip(start).Take((buffer.Length - start)).ToArray();
                    return temp;
                }
                var lengthHEX = buffer.Skip(start).Take(sizeof(int)).ToArray();
                Array.Reverse(lengthHEX);
                length = BitConverter.ToInt32(lengthHEX, 0);
                if (length == 0)
                {
                    return null;
                }
            } while (true);
        }
    }
    public class Session
    {
        public Socket clientSocket;
        public ulong SessionId;
        internal byte[] otherData;
        public DateTime latestRec;
        internal NetworkMngr networkMngr;
        public Task SendAsync(byte[] data)
        {
            TaskCompletionSource tcs = new TaskCompletionSource();
            networkMngr.sendBufferBlock.Add((this, data, tcs));
            return tcs.Task;
        }

    }
}
