using ServerProtocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace DoudizhuServer
{
    public class ClientPeer
    {
        #region 网络部分
        public Socket clientSocket { get; set; }
        public string ip
        {
            get
            {
                try
                {
                    return clientSocket.RemoteEndPoint.ToString();
                }
                catch (ObjectDisposedException) // socket销毁之后仍有可能调用ClientPeer的方法
                {
                    return "(连接已断开)";
                }

            }
        }

        public delegate void ReceiveCompletedDele(ClientPeer client, NetMsg msg);
        public event ReceiveCompletedDele ReceiveCompleted;

        private List<byte> cache = new List<byte>();//当前已接收但是没有处理的数据 
        private NetMsg msg;//同时只能发送一个msg，所以存一下可以重用
        public byte[] receiveBuffer { get; private set; }
        public const int bufferSize = 2048;
        #endregion
        #region 用户信息
        public int userId { get; set; }
        public string username { get; set; }
        #endregion


        public ClientPeer()
        {
            userId = -1;
            receiveBuffer = new byte[bufferSize];
            msg = new NetMsg();
        }

        /// <summary>
        /// 客户端处理数据包的方法
        /// </summary>
        /// <param name="tcpPacket">接收的数据包</param>
        public void HandleReceive(byte[] tcpPacket)
        {
            cache.AddRange(tcpPacket);
            ProcessData();
        }

        /// <summary>
        /// 处理tcp数据报，转化为NetMsg，每解析一个NetMsg，就触发一次ReceiveCompleted事件
        /// </summary>
        /// <remarks>
        /// 一个tcp数据报可能含多个或者一部分数据包，需要自行拆分
        ///tcp数据报：tcp协议一次传输的数据
        ///数据包：包装的一段数据，包含包头和数据段，过于简单就不封装了
        ///</remarks>
        private void ProcessData()
        {
            while (true)
            {
                byte[] data = EncodingTools.Decode(ref cache);
                if (data == null)
                    break;
                NetMsg msg = NetMsg.Deserialize(data);
                ReceiveCompleted(this, msg);
            }
        }


        public void SendNetMsg(int opCode, int subOpCode, object message)
        {
            msg.Reset(opCode, subOpCode, message);
            byte[] data = EncodingTools.Encode(msg.Serialize());

            try
            {
                clientSocket.Send(data);//send不需要异步方式
            }
            catch (Exception)
            {
                Console.WriteLine($"消息发送失败(client ip={this.ip},OpCode={opCode},SubOpCode={subOpCode})");
                throw;
            }

        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public void Disconnect()
        {
            cache.Clear();
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }


    }
}
