using System;
using System.Collections.Generic;
using System.Text;
using ZjhServer;


namespace ServerApplication.GameLogic
{
    interface IGameModule
    {
        void Disconnect(ClientPeer client);
        void ReceiveMessage(ClientPeer client, int subOpCode, object value);
    }
}
