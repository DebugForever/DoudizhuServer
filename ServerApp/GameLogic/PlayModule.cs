using System;
using System.Collections.Generic;
using DoudizhuServer;
using ServerApp.Session;
using ServerProtocol.Code;
using ServerProtocol.SharedCode;

namespace ServerApp.GameLogic
{
    /// <summary>
    /// 管理游戏对局的模块
    /// </summary>
    class PlayModule : IGameModule
    {
        public void Init()
        {
            Sessions.matchSession.MatchRoomAllReady += CreatePlayRoomAndStart;
        }
        public void Disconnect(ClientPeer client)
        {
            //throw new NotImplementedException();
        }

        public void ReceiveNetMsg(ClientPeer client, int subOpCode, object value)
        {
            switch (subOpCode)
            {
                case PlayCode.GrabLandlordCReq:
                    Sessions.playSession.GrabLandlord(client, (bool)value);
                    break;
                case PlayCode.PlayCardCReq:
                    Sessions.playSession.PlayCard(client, (CardSet)value);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 匹配房间玩家全部准备后调用的回调，创建一个游戏房间
        /// </summary>
        private void CreatePlayRoomAndStart(int matchRoomId, List<ClientPeer> clientList)
        {
            PlayRoom playRoom = Sessions.playSession.CreatePlayRoom(clientList);
            Console.WriteLine("匹配房间{0}->游戏房间{1}", matchRoomId, playRoom.roomId);
            playRoom.GameStart();
        }
    }
}
