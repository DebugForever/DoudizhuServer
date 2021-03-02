using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Dto
{
    [Serializable]
    public class UserInfoDto
    {
        public int userId;
        public string username;
        public string iconName;
        public string coin;

        public UserInfoDto()
        {
        }

        public UserInfoDto(int userId, string username, string iconName, string coin)
        {
            this.userId = userId;
            this.username = username;
            this.iconName = iconName;
            this.coin = coin;
        }
    }
}
