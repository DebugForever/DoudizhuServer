using System;
using DoudizhuServer;
using ServerProtocol.Code;
using ServerProtocol.Dto;
using ServerApp.Session;
using ServerApp.Database;


namespace ServerApp.GameLogic
{
    /// <summary>
    /// 管理匹配逻辑的模块
    /// </summary>
    class MatchModule : IGameModule
    {

        public void Init()
        {
            //throw new NotImplementedException();
        }
        public void Disconnect(ClientPeer client)
        {
            //断开连接的时候触发退出房间
            HandleExitRoom(client);
        }

        public void ReceiveNetMsg(ClientPeer client, int subOpCode, object value)
        {
            switch (subOpCode)
            {
                case MatchCode.EnterCReq:
                    HandleEnterRoom(client);
                    break;
                case MatchCode.ExitCReq:
                    HandleExitRoom(client);
                    break;
                case MatchCode.ReadyCReq:
                    HandleReady(client, (bool)value);
                    break;
                default:
                    break;
            }
        }

        #region 处理客户端消息
        private void HandleEnterRoom(ClientPeer client)
        {
            if (Sessions.matchSession.IsMatching(client)) // 已在队列中则不管
                return;

            //先进入房间
            MatchRoom room = Sessions.matchSession.EnterRoom(client);
            UserInfoDto userInfoDto = DatabaseManager.CreateUserInfoDto(client.userId);

            //然后向其他房间内用户广播进入房间信息
            room.Broadcast(OpCode.match, MatchCode.EnterBrd, userInfoDto, client);

            //最后向客户端返回当前的房间信息
            MatchRoomDto matchRoomDto = room.CreateMatchRoomDto();
            client.SendNetMsg(OpCode.match, MatchCode.EnterSRes, matchRoomDto);
        }

        private void HandleExitRoom(ClientPeer client)
        {
            if (!Sessions.matchSession.IsMatching(client)) // 未在房间中则不管
                return;

            //先退出房间
            MatchRoom room = Sessions.matchSession.ExitRoom(client);
            UserInfoDto userInfoDto = DatabaseManager.CreateUserInfoDto(client.userId);

            //然后向其他房间内用户广播退出房间信息
            room.Broadcast(OpCode.match, MatchCode.ExitBrd, userInfoDto, client);
        }

        private void HandleReady(ClientPeer client, bool ready)
        {
            if (!Sessions.matchSession.IsMatching(client))
                return;

            MatchRoom room = Sessions.matchSession.GetRoom(client);
            UserInfoDto userInfoDto = DatabaseManager.CreateUserInfoDto(client.userId);
            if (ready)
            {
                room.Ready(client);
                room.Broadcast(OpCode.match, MatchCode.ReadyBrd, userInfoDto, client);
            }
            else
            {
                room.UnReady(client);
                room.Broadcast(OpCode.match, MatchCode.UnReadyBrd, userInfoDto, client);
            }
        }


        #endregion
    }
}
