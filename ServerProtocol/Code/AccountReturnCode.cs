using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Code
{
    public class AccountReturnCode
    {
        /// <summary>
        /// 请求成功
        /// </summary>
        public const int success = 0;

        /// <summary>
        /// 请求失败
        /// </summary>
        public const int failure = -1;

        /// <summary>
        /// 用户不存在
        /// </summary>
        public const int userNotFound = -2;

        /// <summary>
        /// 用户已存在（用于注册）
        /// </summary>
        public const int userExist = -3;

        /// <summary>
        /// 用户名和密码不匹配
        /// </summary>
        public const int passwordNotMatch = -4;

        /// <summary>
        /// 用户已在线（用于登录）
        /// </summary>
        public const int userOnline = -5;
    }
}
