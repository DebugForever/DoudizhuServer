using DoudizhuServer;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ServerProtocol.Code;
using ServerProtocol.SharedCode;

namespace ServerApp.Session
{
    public class PlaySession
    {
        /// <summary>UserId到PlayRoom的映射 </summary>
        private ConcurrentDictionary<int, PlayRoom> clientRoomDict = new ConcurrentDictionary<int, PlayRoom>();

        /// <summary>RoomId到PlayRoom的映射 </summary>
        private ConcurrentDictionary<int, PlayRoom> roomModelDict = new ConcurrentDictionary<int, PlayRoom>();

        public PlayRoom CreatePlayRoom(List<ClientPeer> clientList)
        {
            PlayRoom room = new PlayRoom(clientList);
            roomModelDict.TryAdd(room.roomId, room);
            foreach (ClientPeer client in clientList)
            {
                if (!clientRoomDict.TryAdd(client.userId, room))
                {
                    throw new ApplicationException(
                        string.Format(
                            "无法开始游戏房间：用户id{0}已在游戏房间{1}内", client.userId, clientRoomDict[client.userId])
                        );
                }
            }
            return room;
        }

        public void GrabLandlord(ClientPeer client, bool isGrab)
        {
            PlayRoom room = GetRoom(client);
            int playerIndex = room.GetClientIndex(client);
            lock (room)
            {
                room.PlayerGrabLandlord(playerIndex, isGrab);
            }
        }


        private PlayRoom GetRoom(ClientPeer client)
        {
            if (clientRoomDict.TryGetValue(client.userId, out var result))
                return result;
            return null;
        }

        public void PlayCard(ClientPeer client, CardSet cardSet)
        {
            //这里直接把不出合并进出牌了
            PlayRoom room = GetRoom(client);
            int playerIndex = room.GetClientIndex(client);
            lock (room)
            {
                if (cardSet == null)
                    room.PlayerPassTurn(playerIndex);
                else
                    room.PlayerPlayCard(playerIndex, cardSet);
            }
        }
    }
}
