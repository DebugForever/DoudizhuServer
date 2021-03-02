using System;
using System.Collections.Generic;
using System.Text;
using ZjhServer;
using ServerProtocol.Code;
using ServerApplication.GameLogic;

namespace ServerApplication
{
    /// <summary>
    /// 网络消息分发中心，负责分发所有接收到的信息
    /// </summary>
    class NetMsgCenter : IApplicationLayer
    {
        private AccountModule accountModule = new AccountModule();
        private ChatModule chatModule = new ChatModule();
        private MatchModule matchModule = new MatchModule();
        private PlayModule playModule = new PlayModule();


        public void Disconnect(ClientPeer client)
        {
            //需要从声明顺序的逆序来Disconnect，有点像析构？
            playModule.Disconnect(client);
            matchModule.Disconnect(client);
            chatModule.Disconnect(client);
            accountModule.Disconnect(client);
        }

        public void ReceiveMessage(ClientPeer client, NetMsg msg)
        {
            switch (msg.opCode)
            {
                case OpCode.account:
                    accountModule.ReceiveMessage(client, msg.subOpCode, msg.value);
                    break;
                case OpCode.chat:
                    chatModule.ReceiveMessage(client, msg.subOpCode, msg.value);
                    break;
                case OpCode.match:
                    matchModule.ReceiveMessage(client, msg.subOpCode, msg.value);
                    break;
                case OpCode.play:
                    playModule.ReceiveMessage(client, msg.subOpCode, msg.value);
                    break;
            }
        }
    }
}
