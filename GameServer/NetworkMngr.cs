using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GameServer
{
    public class NetworkMngr
    {
        Task recvTask;
        bool Stop;
        TcpListener tcpServer;
        BufferBlock<(Session, byte[])> recBufferBlock = new BufferBlock<(Session, byte[])>();
        public delegate Task DataHandler(Session session, byte[] data);
        private DataHandler dataHandler;
        internal BufferBlock<(Session, byte[])> sendBufferBlock = new BufferBlock<(Session, byte[])>();
        public void Init(DataHandler dataHandler)
        {
            recvTask = new Task(Recv);
            Stop = false;
            recvTask.Start();
            this.dataHandler = dataHandler; 
        }

        public void Update()
        {
            IList<(Session, byte[])> list;
            recBufferBlock.TryReceiveAll(out list);
            if (dataHandler != null && list != null)
            {
                foreach(var item in list)
                {
                    dataHandler(item.Item1, item.Item2);
                }
            }

            
        }

        public void Fini()
        {
            Stop = true;
        }

        Dictionary<ulong, Session> clientMap = new Dictionary<ulong, Session>();
        Dictionary<Socket, ulong> socketMap = new Dictionary<Socket, ulong>();
        ulong IdIter = 1;
        byte[] dataBuffer = new byte[1024 * 1024 * 5];
        DateTime now;
        void Recv()
        {
            tcpServer = new TcpListener(System.Net.IPAddress.Any, 2007);
            tcpServer.Start();
            var serverSocket = tcpServer.Server;
            List<Socket> checklist = new List<Socket>();
            List<(ulong, Socket)> waitDelete = new List<(ulong, Socket)>();
            while (!Stop)
            {
                waitDelete.Clear();
                now = DateTime.Now;
                ResetCheckList(ref checklist);
                Socket.Select(checklist, null, null, 1000);
                foreach(var item in checklist)
                {
                    if (item == serverSocket)
                    {
                        var socket = serverSocket.Accept();
                        if (socket != null)
                        {
                            var id = IdIter++;
                            clientMap.Add(id, new Session {clientSocket= socket, SessionId = id , networkMngr =this, latestRec = now });
                            socketMap.Add(socket, id);
                        }
                    }
                    else
                    {
                        var len = 0;
                        try
                        {
                            len = item.Receive(dataBuffer);
                        }
                        catch
                        {
                            var sessionId = socketMap.GetValueOrDefault(item);
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
                            recBufferBlock.Post((session, itemdata));
                        }
                        session.latestRec = now;
                    }
                }
                IList<(Session, byte[])> outListSend;
                if(sendBufferBlock.TryReceiveAll(out outListSend))
                {
                    foreach(var pair in outListSend)
                    {
                        if (socketMap.GetValueOrDefault(pair.Item1.clientSocket) != 0)
                        {
                            pair.Item1.clientSocket.BeginSend(pair.Item2, 0, pair.Item2.Length, 0, null, null);
                        }
                    }
                }
                foreach(var pair in clientMap)
                {
                    if ((now - pair.Value.latestRec).Seconds >= 5)
                    {
                        waitDelete.Add((pair.Key, pair.Value.clientSocket));
                    }
                }
                waitDelete.ForEach(pair => { clientMap.Remove(pair.Item1); socketMap.Remove(pair.Item2); });
                Thread.Sleep(0);
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
       
        public void Send(byte[] data)
        {
            networkMngr.sendBufferBlock.Post((this, data));
        }

    }
}
