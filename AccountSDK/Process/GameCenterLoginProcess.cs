namespace Habby.Account.Process
{
#if UNITY_IOS
    
    public class GameCenterLoginProcess : ProcessBase<object>
    {
        public HabbyAccount_GameCenter account;
    }
    
#endif
}