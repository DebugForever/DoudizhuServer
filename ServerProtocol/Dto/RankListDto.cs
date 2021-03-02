using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerProtocol.Dto
{
    [Serializable]
    public class RankListDto
    {
        public List<RankItemDto> list = new List<RankItemDto>();
    }
}
