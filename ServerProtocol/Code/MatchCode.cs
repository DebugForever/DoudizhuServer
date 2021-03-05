using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Code
{
    public class MatchCode
    {
        /// <summary>进入房间客户端请求</summary>
        public const int EnterCReq = 0;
        /// <summary>进入房间服务器响应</summary>
        public const int EnterSRes = 1;
        /// <summary>进入房间服务器广播</summary>
        public const int EnterBrd = 2;

        /// <summary>退出房间客户端请求</summary>
        public const int ExitCReq = 3;
        /// <summary>退出房间服务器响应</summary>
        public const int ExitSRes = 4;
        /// <summary>退出房间服务器广播</summary>
        public const int ExitBrd = 5;

        /// <summary>准备客户端请求</summary>
        public const int ReadyCReq = 6;
        /// <summary>准备服务器响应</summary>
        public const int ReadySRes = 7;
        /// <summary>准备服务器广播</summary>
        public const int ReadyBrd = 8;
        /// <summary>取消准备服务器广播</summary>
        public const int UnReadyBrd = 9;

        /// <summary>开始游戏服务器广播</summary>
        public const int GameStartBrd = 10;
    }
}
