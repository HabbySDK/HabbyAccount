
using Habby.Tool.Http.Tool;

namespace Habby.Account
{

    public sealed class AccountRequestPath : RequestPathObjectBase
    {
        public AccountRequestPath(string pPath) : base(HabbyAccountManager.ServerUrl, pPath)
        {
            
        }
    }
}