using ServerProtocol.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerProtocol.Dto
{
    /// <summary>
    /// 抢地主结束数据传输模型
    /// </summary>
    [Serializable]
    public class GrabLandlordEndDto
    {
        /// <summary>
        /// 成为地主的玩家编号
        /// </summary>
        public int landlordIndex;

        /// <summary>
        /// 三张底牌
        /// </summary>
        public Card[] UnderCards;

        public GrabLandlordEndDto(int landlordIndex, Card[] underCards)
        {
            this.landlordIndex = landlordIndex;
            UnderCards = underCards;
        }
    }
}
