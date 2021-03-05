using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// DTO: DataTranferObject
// DTO是用于传输网络数据的对象
namespace ServerProtocol.Dto
{
    [Serializable]
    public class AccountDto
    {
        public string username;
        public string passwordHash;//因为只是一个Demo，所以不需要过分考虑安全方面，就没有加盐之类的操作了，直接哈希了

        public AccountDto()
        {

        }

        public AccountDto(string userName, string passwordHash)
        {
            this.username = userName;
            this.passwordHash = passwordHash;
        }
    }


}
