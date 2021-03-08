using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Code
{
    public class PlayCode
    {
        /// <summary>发牌广播</summary>
        public const int DealCardBrd = 0;

        /// <summary>抢地主客户端请求</summary>
        public const int GrabLandlordCReq = 1;
        /// <summary>抢地主服务器响应</summary>
        public const int GrabLandlordSRes = 2;
        /// <summary>抢地主广播</summary>
        public const int GrabLandlordBrd = 3;
        /// <summary>不抢地主广播</summary>
        public const int NoGrabLandlordBrd = 4;

        /// <summary>抢地主结束广播</summary>
        public const int GrabLandlordEndBrd = 5;

        /// <summary>添加牌广播</summary>
        /// <remarks>抢地主成功时发送</remarks>
        public const int AddCardBrd = 6;

        /// <summary>开始回合广播</summary>
        public const int StartTurnBrd = 7;

        /// <summary>出牌客户端请求（传null表示不出）</summary>
        public const int PlayCardCReq = 8;
        /// <summary>出牌服务器响应</summary>
        public const int PlayCardSRes = 9;
        /// <summary>出牌广播</summary>
        public const int PlayCardBrd = 10;
        /// <summary>不出牌广播</summary>
        public const int PassPlayCardBrd = 11;

        /// <summary>结束回合客户端请求</summary>
        public const int EndTurnCReq = 12;
        /// <summary>结束回合服务器响应</summary>
        public const int EndTurnSRes = 13;
        /// <summary>结束回合广播</summary>
        public const int EndTurnBrd = 14;

        /// <summary>游戏结束广播</summary>
        public const int GameEndBrd = 15;


    }
}
