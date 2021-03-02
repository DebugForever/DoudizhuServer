using ServerProtocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DoudizhuServer
{
    public class ServerPeer
    {
        private Socket serverSocket;
        private ClientPeerPool pool;
        private Semaphore semaphore;
        private IApplicationLayer app;

        public void SetApplicationLayer(IApplicationLayer app)
        {
            this.app = app;
        }

        public void StartServer(string ip, int port, int maxClient)
        {
            semaphore = new Semaphore(maxClient, maxClient);
            pool = new ClientPeerPool(maxClient);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            for (int i = 0; i < maxClient; i++)
            {
                ClientPeer client = new ClientPeer();
                client.ReceiveCompleted += OnRecieve;
                pool.Push(client);
            }

            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            serverSocket.Listen(maxClient);

            app.Init();//初始化app
            Console.WriteLine("服务器启动成功");
            StartAccept(null);
        }

        #region 处理连接
        /// <summary>
        /// 接受客户端的连接
        /// </summary>
        /// <param name="eventArgs"></param>
        private void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += EventArgs_Completed;
            }

            bool result = serverSocket.AcceptAsync(eventArgs);
            if (result == false)
            {
                HandleAccept(eventArgs);
            }
        }

        /// <summary>
        /// 处理客户端的(异步)连接请求
        /// </summary>
        /// <param name="eventArgs"></param>
        private void HandleAccept(SocketAsyncEventArgs eventArgs)
        {
            semaphore.WaitOne();
            ClientPeer client = pool.Pop();
            client.clientSocket = eventArgs.AcceptSocket;
            Console.WriteLine(string.Format("已连接客户端 {0}", eventArgs.AcceptSocket.RemoteEndPoint));

            //开始接受消息
            StartReceive(client);

            eventArgs.AcceptSocket = null;
            StartAccept(eventArgs);
        }


        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            HandleAccept(e);
        }
        #endregion 处理连接
        #region 接收数据

        private void StartReceive(ClientPeer client)
        {
            client.clientSocket.BeginReceive(client.receiveBuffer, 0, ClientPeer.bufferSize, SocketFlags.None, ReceiveCallback, client);
        }

        /// <summary>
        /// 处理（异步）接收数据
        /// </summary>
        private void ReceiveCallback(IAsyncResult ar)
        {
            ClientPeer client = ar.AsyncState as ClientPeer;
            int receiveLen;
            try
            {
                receiveLen = client.clientSocket.EndReceive(ar);
            }
            catch (SocketException e)
            {
                Disconnect(client, e.Message);
                return;
            }

            if (receiveLen > 0)
            {
                //数据接收成功
                byte[] tcpPacket = new byte[receiveLen];
                Console.WriteLine("received {0} bytes from {1}", receiveLen, client.ip);
                Buffer.BlockCopy(client.receiveBuffer, 0, tcpPacket, 0, receiveLen);
                client.HandleReceive(tcpPacket);

                //继续接收
                StartReceive(client);
            }
            else
            {
                Disconnect(client, "接收到零长度数据，自动断开连接");
            }
        }



        /// <summary>
        /// 客户端接收数据后的回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void OnRecieve(ClientPeer client, NetMsg msg)
        {
            app.ReceiveMessage(client, msg);
        }

        #endregion 接收数据
        #region 断开连接
        /// <summary>
        /// 主动断开连接
        /// </summary>
        /// <param name="client">要断开连接的客户端对象</param>
        /// <param name="reason">断开连接的理由</param>
        private void Disconnect(ClientPeer client, string reason)
        {
            if (client == null)
            {
                throw new ArgumentException("参数client为空，断开连接失败");
            }

            Console.WriteLine(string.Format("断开与客户端{0}的连接，理由是{1}。", client.ip, reason));
            client.Disconnect();
            app.Disconnect(client);

            pool.Push(client);
            semaphore.Release();
        }
        #endregion 断开连接
    }
}
