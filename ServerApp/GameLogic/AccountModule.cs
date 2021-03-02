using MySql.Data.MySqlClient;
using ServerApp.Database;
using ServerProtocol.Code;
using ServerProtocol.Dto;
using System;
using DoudizhuServer;
using ServerApp.Manager;

namespace ServerApp.GameLogic
{
    /// <summary>
    /// 管理账号的模块
    /// </summary>
    class AccountModule : IGameModule
    {
        public void Init()
        {
            //improve
            //清除所有用户在线状态，防止服务器宕机导致用户假在线
            //其实在线状态应该是程序的变量，用一个类管理，而不是存在数据库里
            DatabaseManager.ClearOnline();
        }

        public void Disconnect(ClientPeer client)
        {
            DatabaseManager.Logout(client);
        }

        public void ReceiveNetMsg(ClientPeer client, int subOpCode, object value)
        {
            switch (subOpCode)
            {
                case AccountCode.registerCReq:
                    HandleRegister(client, value as AccountDto);
                    break;
                case AccountCode.loginCReq:
                    HandleLogin(client, value as AccountDto);
                    break;
                case AccountCode.getUserInfoCReq:
                    HandleGetUserInfo(client);
                    break;
                case AccountCode.getRankListCReq:
                    HandleGetRankList(client);
                    break;
                default:
                    throw new ArgumentException("subOpCode not exist!");
            }
        }

        #region 处理客户端消息
        /// <summary>
        /// 处理客户端的注册请求
        /// </summary>
        private void HandleRegister(ClientPeer client, AccountDto dto)
        {
            SingleExec.Exec(() =>
            {
                if (DatabaseManager.IsUserNameExist(dto.username))
                {
                    client.SendNetMsg(OpCode.account, AccountCode.registerSRes, AccountReturnCode.userExist);
                    return;
                }

                try
                {
                    DatabaseManager.CreateUser(dto.username, dto.passwordHash);
                    client.SendNetMsg(OpCode.account, AccountCode.registerSRes, AccountReturnCode.success);
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e);
                }
            });
        }


        /// <summary>
        /// 处理登录请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dto"></param>
        private void HandleLogin(ClientPeer client, AccountDto dto)
        {
            SingleExec.Exec(() =>
            {
                if (!DatabaseManager.IsUserNameExist(dto.username))
                {
                    client.SendNetMsg(OpCode.account, AccountCode.loginSRes, AccountReturnCode.userNotFound);
                    return;
                }

                if (DatabaseManager.IsOnline(dto.username))
                {
                    client.SendNetMsg(OpCode.account, AccountCode.loginSRes, AccountReturnCode.userOnline);
                    return;
                }

                if (DatabaseManager.IsPasswordMatch(dto.username, dto.passwordHash))
                {
                    //设置登录状态
                    DatabaseManager.Login(dto.username, client);

                    //登录成功
                    client.SendNetMsg(OpCode.account, AccountCode.loginSRes, AccountReturnCode.success);
                }
                else
                {
                    client.SendNetMsg(OpCode.account, AccountCode.loginSRes, AccountReturnCode.passwordNotMatch);
                }
            });
        }

        /// <summary>
        /// 获取当前客户端登录用户的信息
        /// </summary>
        /// <param name="client"></param>
        private void HandleGetUserInfo(ClientPeer client)
        {
            SingleExec.Exec(() =>
            {
                UserInfoDto dto = DatabaseManager.CreateUserInfoDto(client.userid);
                client.SendNetMsg(OpCode.account, AccountCode.getUserInfoSRes, dto);
            });
        }

        private void HandleGetRankList(ClientPeer client)
        {
            SingleExec.Exec(() =>
            {
                RankListDto dto = DatabaseManager.GetRankListDto();
                client.SendNetMsg(OpCode.account, AccountCode.getRankListSRes, dto);
            });
        }

        #endregion

    }
}
