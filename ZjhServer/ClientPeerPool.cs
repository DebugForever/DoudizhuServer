using System.Collections.Generic;


namespace DoudizhuServer
{
    /// <summary>
    /// 客户端对象池
    /// </summary>
    internal class ClientPeerPool
    {
        private Queue<ClientPeer> pool;
        public ClientPeerPool(int maxClient)
        {
            pool = new Queue<ClientPeer>(maxClient);
        }

        /// <summary>
        /// 获取一个客户端对象，信号量由外部控制，保证不会pop空队列
        /// </summary>
        public ClientPeer Pop()
        {
            return pool.Dequeue();
        }

        /// <summary>
        /// 归还一个客户端对象
        /// </summary>
        /// <param name="client"></param>
        public void Push(ClientPeer client)
        {
            pool.Enqueue(client);
        }
    }
}
