using System;
using System.Collections.Generic;
using System.Text;
using ZjhServer;

namespace ServerApplication.GameLogic
{
    /// <summary>
    /// 管理匹配逻辑的模块
    /// </summary>
    class MatchModule : IGameModule
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
