using System;
using DoudizhuServer;

namespace ServerApp.GameLogic
{
    /// <summary>
    /// 管理游戏对局的模块
    /// </summary>
    class PlayModule : IGameModule
    {
        public void Init()
        {
            //throw new NotImplementedException();
        }
        public void Disconnect(ClientPeer client)
        {
            //throw new NotImplementedException();
        }

        public void ReceiveNetMsg(ClientPeer client, int subOpCode, object value)
        {
            throw new NotImplementedException();
        }
    }
}
