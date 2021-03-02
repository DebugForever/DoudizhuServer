using System;
using System.Collections.Generic;
using System.Text;
using ZjhServer;

namespace ServerApplication.GameLogic
{
    /// <summary>
    /// 管理账号的模块
    /// </summary>
    class AccountModule : IGameModule
    {
        public void Disconnect(ClientPeer client)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(ClientPeer client, int subOpCode, object value)
        {
            throw new NotImplementedException();
        }
    }
}
