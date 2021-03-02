using ServerApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZjhServer;

namespace ServerApplication
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            ServerPeer server = new ServerPeer();
            server.SetApplicationLayer(new NetMsgCenter());
            server.StartServer("127.0.0.1", 6666, 10);

            //阻塞主线程，防止退出
            Console.ReadLine();
        }
    }
}
