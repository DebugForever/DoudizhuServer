using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Dto
{
    [Serializable]
    public class RankItemDto
    {
        public string username;
        public int coin;

        public RankItemDto()
        {
        }

        public RankItemDto(string username, string iconName, int coin)
        {
            this.username = username;
            this.coin = coin;
        }
    }
}
