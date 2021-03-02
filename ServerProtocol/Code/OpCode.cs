namespace ServerProtocol.Code
{
    //这个类能不能换成枚举类型？
    public class OpCode
    {
        /// <summary>
        /// 账号模块
        /// </summary>
        public const int account = 0;

        /// <summary>
        /// 匹配模块
        /// </summary>
        public const int match = 1;

        /// <summary>
        /// 聊天模块
        /// </summary>
        public const int chat = 2;

        /// <summary>
        /// 游戏模块
        /// </summary>
        public const int play = 3;
    }
}
