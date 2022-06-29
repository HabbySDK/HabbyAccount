using Habby.Tool.Http;
namespace Habby.Account
{
    public class AccountHttpManager : HttpManager<AccountHttpManager>
    {
        public AccountHttpManager()
        {
            Tag = "AccountSDK";
        }
    }
}
