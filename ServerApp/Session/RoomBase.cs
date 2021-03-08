using DoudizhuServer;
using System.Threading;

namespace ServerApp.Session
{
    public abstract class RoomBase
    {
        /// <summary>房间容量</summary>
        protected const int capacity = 3;

        /// <summary>房间id，创建时线程安全</summary>
        public int roomId { get; private set; }

        /// <summary>下一个房间的id</summary>
        private static int nextId = 0;

        public RoomBase()
        {
            roomId = Interlocked.Increment(ref nextId);
        }

        public abstract void Broadcast(int opCode, int subOpCode, object message, ClientPeer ignoreClient = null);

    }
}
