using ServerProtocol;

namespace DoudizhuServer
{
    public interface IApplicationLayer
    {
        /// <summary>
        /// 启动前初始化
        /// </summary>
        void Init();

        /// <summary>
        /// socket断开连接时调用
        /// </summary>
        /// <param name="client">断开连接的ClientPeer</param>
        void Disconnect(ClientPeer client);

        /// <summary>
        /// 处理收到的信息
        /// </summary>
        /// <param name="client">发来消息的ClientPeer</param>
        /// <param name="msg"></param>
        void ReceiveMessage(ClientPeer client, NetMsg msg);
    }
}
