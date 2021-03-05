using MySql.Data.MySqlClient;
using ServerApp.Manager;
using ServerProtocol.Dto;
using System;
using System.Collections.Generic;
using DoudizhuServer;

namespace ServerApp.Database
{
    public static class DatabaseManager
    {
        private static MySqlConnection connection;

        /// <summary>
        /// 连接数据库
        /// </summary>
        public static void Connect()
        {
            MySqlConnectionStringBuilder connectionBuilder = new MySqlConnectionStringBuilder();
            connectionBuilder.UserID = "root";
            connectionBuilder.Database = "zjhserver";
            connectionBuilder.Server = "127.0.0.1";
            connectionBuilder.Password = "123";
            connectionBuilder.Port = 3306;
            connection = new MySqlConnection(connectionBuilder.ToString());
            connection.Open();

        }

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsUserNameExist(string userName)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select username from user_info where username=@name";
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();
            return result;
        }

        /// <summary>
        /// 创建一个用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passwordHash"></param>
        public static void CreateUser(string userName, string passwordHash)
        {
            Random random = new Random();

            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "insert into user_info(userName,passwordHash,online,iconName) values(@name,@pwHash,0,@iconName);";
            cmd.Parameters.AddWithValue("name", userName);
            cmd.Parameters.AddWithValue("pwHash", passwordHash);
            cmd.Parameters.AddWithValue("iconName", "headIcon_" + random.Next(0, 19));
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 检查用户名和密码是否匹配
        /// </summary>
        /// <param name="username"></param>
        /// <param name="passwordHash"></param>
        public static bool IsPasswordMatch(string username, string passwordHash)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from user_info where username=@name";//sql不区分大小写，所以这里的username和上面不一样也没关系
            cmd.Parameters.AddWithValue("name", username);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                bool result = (passwordHash == reader.GetString("passwordHash"));
                reader.Close();
                return result;
            }
            else
            {
                reader.Close();
                return false;
            }
        }

        /// <summary>
        /// 查询指定用户是否在线
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool IsOnline(string username)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select online from user_info where username=@name";
            cmd.Parameters.AddWithValue("name", username);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            bool result = reader.GetBoolean("online");
            reader.Close();
            return result;
        }

        /// <summary>
        /// 设置指定用户的在线状态
        /// </summary>
        /// <param name="username"></param>
        /// <param name="online"></param>
        private static void SetOnline(string username, bool online)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "update user_info set online=@online where username=@name";
            cmd.Parameters.AddWithValue("online", online);
            cmd.Parameters.AddWithValue("name", username);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 将所有用户的在线状态重置为false
        /// </summary>
        public static void ClearOnline()
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "update user_info set online=0 where id>=0";//必须加上where语句，否则因为安全设置无法执行
            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// 指定用户登录操作，并为ClientPeer写入登录的用户信息 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="client"></param>
        public static void Login(string username, ClientPeer client)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from user_info where username=@name";
            cmd.Parameters.AddWithValue("name", username);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows)
            {
                int id = reader.GetInt32("id");
                reader.Close();//在进行任何数据库操作前，应关闭reader
                LoginStateManager.Login(id, username, client);
                SetOnline(username, true);
            }
            else
            {
                reader.Close();
            }
        }

        /// <summary>
        /// 指定用户下线操作，并为已绑定的ClientPeer清除登录的用户信息 
        /// </summary>
        /// 下线一般是由socket发起的，所以只传ClientPeer即可
        public static void Logout(ClientPeer client)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "update user_info set online=0 where id=@id";
            cmd.Parameters.AddWithValue("id", client.userId);
            cmd.ExecuteNonQuery();
            LoginStateManager.Logout(client);//注意logout之后会将userID重置
        }

        /// <summary>
        /// 获取指定用户的信息，以UserInfoDto形式输出，不存在id返回null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static UserInfoDto CreateUserInfoDto(int id)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select * from user_info where id=@id";
            cmd.Parameters.AddWithValue("id", id);
            MySqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            if (reader.HasRows)
            {
                UserInfoDto dto = new UserInfoDto();
                dto.userId = id;
                dto.username = reader.GetString("username");
                dto.iconName = reader.GetString("iconName");
                dto.coin = reader.GetInt32("coin");
                reader.Close();
                return dto;
            }
            else
            {
                reader.Close();
                return null;
            }
        }

        public static RankListDto GetRankListDto(int maxCount = 10)
        {
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "select username,coin from user_info order by coin desc";
            MySqlDataReader reader = cmd.ExecuteReader();
            RankListDto resultList = new RankListDto();
            RankItemDto item = new RankItemDto();
            int count = 0;

            if (reader.HasRows)
            {
                while (reader.Read() && count < maxCount)
                {
                    item.username = reader.GetString("username");
                    item.coin = reader.GetInt32("coin");
                    resultList.list.Add(item);
                    count += 1;
                }
            }
            reader.Close();
            return resultList;
        }

    }
}
