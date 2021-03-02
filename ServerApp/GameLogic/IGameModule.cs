using DoudizhuServer;


namespace ServerApp.GameLogic
{
    interface IGameModule
    {
        void Init();
        void Disconnect(ClientPeer client);
        void ReceiveNetMsg(ClientPeer client, int subOpCode, object value);
    }
}
