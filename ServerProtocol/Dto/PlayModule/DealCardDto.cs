using ServerProtocol.SharedCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerProtocol.Dto
{
    [Serializable]
    public class DealCardDto
    {
        public Card[] cards;

        public DealCardDto(Card[] cards)
        {
            this.cards = cards;
        }
    }
}
