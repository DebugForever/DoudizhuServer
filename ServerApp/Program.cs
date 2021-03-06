﻿using ServerApp.Database;
using ServerApp;
using System;
using DoudizhuServer;


namespace ServerApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            DatabaseManager.Connect();
            ServerPeer server = new ServerPeer();
            server.SetApplicationLayer(new NetMsgCenter());
            server.StartServer("127.0.0.1", 6666, 10);

            //阻塞主线程，防止退出
            Console.ReadLine();
        }
    }
}
