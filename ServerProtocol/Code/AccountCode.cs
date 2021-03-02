using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Code
{
    public class AccountCode
    {
        /// <summary>
        ///客户端注册请求
        /// </summary>
        public const int registerCReq = 0;

        /// <summary>
        /// 服务器响应注册请求
        /// </summary>
        public const int registerSRes = 1;

        /// <summary>
        /// 客户端登录请求
        /// </summary>
        public const int loginCReq = 2;

        /// <summary>
        /// 服务器响应登录请求
        /// </summary>
        public const int loginSRes = 3;

        /// <summary>
        /// 客户端获取用户信息请求
        /// </summary>
        public const int getUserInfoCReq = 4;

        /// <summary>
        /// 服务器响应获取用户信息请求
        /// </summary>
        public const int getUserInfoSRes = 5;

        /// <summary>
        /// 客户端获取排行榜请求
        /// </summary>
        public const int getRankListCReq = 6;

        /// <summary>
        /// 服务器响应获取排行榜请求
        /// </summary>
        public const int getRankListSRes = 7;


    }
}
