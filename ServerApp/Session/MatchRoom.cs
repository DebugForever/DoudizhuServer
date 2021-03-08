using DoudizhuServer;
using ServerApp.Database;
using ServerProtocol;
using ServerProtocol.Code;
using ServerProtocol.Dto;
using System;
using System.Collections.Generic;
using System.Threading;


namespace ServerApp.Session
{
    /// <summary>
    /// 匹配房间管理
    /// </summary>
    public class MatchRoom : RoomBase
    {
        /// <summary>
        /// userId到房间用户信息的映射
        /// </summary>
        private Dictionary<int, RoomUserInfo> userDict = new Dictionary<int, RoomUserInfo>();

        /// <summary>
        /// 用户顺序列表，以id存储，表示进入房间的顺序
        /// </summary>
        private List<int> userOrderList = new List<int>();

        public event Action<int, List<ClientPeer>> AllReady;

        /// <summary>
        /// 用户进入房间
        /// </summary>
        /// <param name="client"></param>
        public void EnterRoom(ClientPeer client)
        {
            if (!userDict.ContainsKey(client.userId))
            {
                userDict.Add(client.userId, new RoomUserInfo(client, false));
                userOrderList.Add(client.userId);
                Console.WriteLine(string.Format("用户ID{0}进入房间{1}，当前房间人数：{2}", client.userId, roomId, GetClientList().Count));
            }
        }

        /// <summary>
        /// 用户退出房间
        /// </summary>
        /// <param name="client"></param>
        public void ExitRoom(ClientPeer client)
        {
            if (userDict.ContainsKey(client.userId))
            {
                userDict.Remove(client.userId);
                userOrderList.Remove(client.userId);
                Console.WriteLine(string.Format("用户ID{0}退出房间{1}，当前房间人数：{2}", client.userId, roomId, GetClientList().Count));
            }
        }

        /// <summary>
        /// 用户准备
        /// </summary>
        /// <remarks>如果房间里没有这个人，则无效果</remarks>
        public void Ready(ClientPeer client)
        {
            if (userDict.ContainsKey(client.userId))
            {
                userDict[client.userId].ready = true;
                Console.WriteLine(string.Format("房间{0}:用户ID{1}准备", roomId, client.userId));
                if (IsAllReady())
                {
                    Broadcast(OpCode.match, MatchCode.GameStartBrd, null);
                    AllReady(roomId, GetClientList());
                }
            }
        }

        /// <summary>
        /// 用户取消准备
        /// </summary>
        /// <remarks>如果房间里没有这个人，则无效果</remarks>
        public void UnReady(ClientPeer client)
        {
            if (userDict.ContainsKey(client.userId))
            {
                userDict[client.userId].ready = false;
                Console.WriteLine(string.Format("房间{0}:用户ID{1}取消准备", roomId, client.userId));
            }
        }

        /// <summary>
        /// 房间是否为满
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return userDict.Count == capacity;
        }

        /// <summary>
        /// 房间是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return userDict.Count == 0;
        }

        /// <summary>
        /// 房间是否满员并全部准备
        /// </summary>
        /// <returns></returns>
        public bool IsAllReady()
        {
            if (!IsFull())
                return false;
            foreach (var item in userDict)
            {
                if (!item.Value.ready)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 清空房间，重置为初始状态
        /// </summary>
        public void Clear()
        {
            userDict.Clear();
            userOrderList.Clear();
        }

        public List<ClientPeer> GetClientList()
        {
            List<ClientPeer> result = new List<ClientPeer>();
            foreach (var item in userDict.Values)
            {
                result.Add(item.client);
            }
            return result;
        }

        public override void Broadcast(int opCode, int subOpCode, object message, ClientPeer ignoreClient = null)
        {
            foreach (var item in userDict)
            {
                ClientPeer client = item.Value.client;
                if (client == ignoreClient)
                    continue;
                client.SendNetMsg(opCode, subOpCode, message);
            }
        }

        public MatchRoomDto CreateMatchRoomDto()
        {
            List<MatchRoomUserInfoDto> userInfoDtoList = new List<MatchRoomUserInfoDto>();
            for (int i = 0; i < userOrderList.Count; i++)
            {
                int userId = userOrderList[i];
                RoomUserInfo info = userDict[userId];
                UserInfoDto userInfoDto = DatabaseManager.CreateUserInfoDto(info.client.userId);
                MatchRoomUserInfoDto userDto = new MatchRoomUserInfoDto(userInfoDto, info.ready, i);
                userInfoDtoList.Add(userDto);
            }
            return new MatchRoomDto(userInfoDtoList);
        }
    }

    /// <summary>
    /// 准备情况的数据类
    /// </summary>
    internal class RoomUserInfo
    {
        public ClientPeer client;
        public bool ready;

        public RoomUserInfo(ClientPeer client, bool ready)
        {
            this.client = client;
            this.ready = ready;
        }
    }
}
