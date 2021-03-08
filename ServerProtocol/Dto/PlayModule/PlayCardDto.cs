using ServerProtocol.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerProtocol.Dto
{
    [Serializable]
    public class PlayCardDto
    {
        public CardSet cardSet;

        /// <summary>服务器发给客户端用</summary>
        public int playerIndex;

        public PlayCardDto(CardSet cardSet, int playerIndex = -1)
        {
            this.cardSet = cardSet;
            this.playerIndex = playerIndex;
        }
    }
}
