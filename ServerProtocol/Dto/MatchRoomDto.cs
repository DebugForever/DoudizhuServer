using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Dto
{
    /// <summary>
    /// 匹配房间中的用户的传输模型
    /// </summary>
    [Serializable]
    public class MatchRoomUserInfoDto
    {
        public UserInfoDto userInfo;
        public bool ready;

        /// <summary>
        /// 在房间中的位置编号
        /// </summary>
        /// <remarks>这里指服务器中的位置编号。客户端中位置不一样，客户端当前账号永远是1号位。</remarks>
        public int placeIndex;

        public MatchRoomUserInfoDto()
        {

        }

        public MatchRoomUserInfoDto(UserInfoDto userInfo, bool ready, int placeIndex)
        {
            this.userInfo = userInfo;
            this.ready = ready;
            this.placeIndex = placeIndex;
        }
    }

    /// <summary>
    /// 匹配房间传输模型
    /// </summary>
    [Serializable]
    public class MatchRoomDto
    {
        public List<MatchRoomUserInfoDto> userList;

        public MatchRoomDto()
        {

        }

        public MatchRoomDto(List<MatchRoomUserInfoDto> userList)
        {
            this.userList = userList;
        }
    }
}
