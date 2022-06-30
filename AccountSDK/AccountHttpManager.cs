using Habby.Business;
using Habby.Tool;
using Habby.Tool.Http;
namespace Habby.Account
{
    public class AccountHttpManager : HttpManager<AccountHttpManager>
    {
        public AccountHttpManager()
        {
            Tag = "AccountSDK";
            SetPublicHeader("SDKVersion",HabbyAccountManager.SDKVersion);
        }
    }
}
