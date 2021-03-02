using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoudizhuServer;

namespace ServerApp.Manager
{
    /// <summary>
    /// 管理在线用户和ClientPeer之间的映射关系
    /// </summary>
    public static class LoginStateManager
    {
        private static Dictionary<int, ClientPeer> useridClientPeerDict = new Dictionary<int, ClientPeer>();

        /// <summary>
        /// 用户上线，并绑定ClientPeer
        /// </summary>
        public static void Login(int userid, string username, ClientPeer client)
        {
            client.userid = userid;
            client.username = username;
            if (!useridClientPeerDict.ContainsKey(userid))
            {
                useridClientPeerDict.Add(userid, client);
            }
        }

        /// <summary>
        /// 用户下线，并解除绑定ClientPeer
        /// </summary>
        public static void Logout(ClientPeer client)
        {
            //remove失败会返回false，remove不存在的key不会引发异常
            useridClientPeerDict.Remove(client.userid);
            client.userid = -1;
            client.username = null;
        }

        public static ClientPeer GetClientByUserid(int userid)
        {
            ClientPeer client;
            bool hasValue = useridClientPeerDict.TryGetValue(userid, out client);
            if (!hasValue)
                client = null;
            return client;
        }
    }
}
