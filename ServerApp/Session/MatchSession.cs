using DoudizhuServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace ServerApp.Session
{
    /// <summary>
    /// 管理运行时匹配房间
    /// </summary>
    public class MatchSession
    {
        /// <summary>UserId到RoomId的映射，包含的userId表示已经在匹配中</summary>
        private ConcurrentDictionary<int, MatchRoom> clientRoomDict = new ConcurrentDictionary<int, MatchRoom>();

        /// <summary>RoomId到MatchRoom的映射，空房间会被移除</summary>
        private ConcurrentDictionary<int, MatchRoom> roomModelDict = new ConcurrentDictionary<int, MatchRoom>();

        /// <summary>未满的房间队列</summary>
        private ConcurrentQueue<MatchRoom> availableRoomQueue = new ConcurrentQueue<MatchRoom>();

        /// <summary>空房间队列，用于重用对象</summary>
        private ConcurrentQueue<MatchRoom> emptyRoomQueue = new ConcurrentQueue<MatchRoom>();

        public event Action<int, List<ClientPeer>> MatchRoomFull;

        /// <summary>
        /// 加入一个未满的房间并返回该房间
        /// </summary>
        /// <remarks>没有未满的房间会创建一个</remarks>
        public MatchRoom EnterRoom(ClientPeer client)
        {
            //注意线程安全
            MatchRoom room = null;
            if (!availableRoomQueue.TryDequeue(out room)) // 尝试获取可进入的房间
            {
                //没有可进入的房间，创建或者获取一个空房间
                if (!emptyRoomQueue.TryDequeue(out room))
                {
                    room = new MatchRoom();
                    if (!roomModelDict.TryAdd(room.roomId, room))
                        throw new ApplicationException("无法创建房间：房间ID冲突");
                }
            }

            lock (room)
            {
                room.EnterRoom(client);
                clientRoomDict.TryAdd(client.userId, room);

                if (!room.IsFull())
                    availableRoomQueue.Enqueue(room);
                else
                    Console.WriteLine(string.Format("房间{0}已满。(todo:此处应触发一个事件)", room.roomId));
            }
            return room;
        }

        /// <summary>
        /// 退出当前房间并返回该房间
        /// </summary>
        public MatchRoom ExitRoom(ClientPeer client)
        {
            if (clientRoomDict.TryGetValue(client.userId, out MatchRoom room))
            {
                lock (room)
                {
                    room.ExitRoom(client);
                }
                if (!clientRoomDict.TryRemove(client.userId, out var _))
                    throw new ApplicationException("无法退出房间：重复退出");

                //退出房间后，这个房间就可以加入了。如果房间为空，那么应该移除该房间并加入空房间队列
                if (room.IsEmpty())
                {
                    if (!roomModelDict.TryRemove(room.roomId, out var _))
                        throw new ApplicationException("无法移除房间：重复移除");
                    emptyRoomQueue.Enqueue(room);
                }
                else
                {
                    availableRoomQueue.Enqueue(room);
                }
            }
            else
                room = null;

            return room;
        }

        /// <summary>
        /// 该用户是否在匹配房间中
        /// </summary>
        public bool IsMatching(ClientPeer client)
        {
            return clientRoomDict.ContainsKey(client.userId);
        }

        public MatchRoom GetRoom(ClientPeer client)
        {
            if (clientRoomDict.TryGetValue(client.userId, out MatchRoom value))
                return value;
            return null;
        }

        public void DestroyRoom(MatchRoom room)
        {
            foreach (ClientPeer client in room.GetClients())
            {
                if (!clientRoomDict.TryRemove(client.userId, out var _))
                    throw new ApplicationException("无法销毁房间：用户丢失");
            }

            if (roomModelDict.TryRemove(room.roomId, out var _))
                throw new ApplicationException("无法销毁房间：房间丢失");

            emptyRoomQueue.Enqueue(room);
        }
    }
}
