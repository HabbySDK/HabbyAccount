#if UNITY_ANDROID && !GOOGLEPLAY_LOGIN
#define GOOGLEPLAY_LOGIN
#endif


using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Habby.Account.Data;

namespace Habby.Account.ThirdPartInterface
{
    public class GMSInterface
    {
#if GOOGLEPLAY_LOGIN
        public PlayGamesClientConfiguration.Builder config = new PlayGamesClientConfiguration.Builder();
#endif
        public bool refAuthcode = false;
        public void Init()
        {
#if GOOGLEPLAY_LOGIN
            var tbuild = config.RequestServerAuthCode(refAuthcode);
            
            PlayGamesPlatform.InitializeInstance(tbuild.Build());
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
#endif
        }
        public AccountDataGooglePlay GetChannelInfo()
        {

            AccountDataGooglePlay ret = new AccountDataGooglePlay(); 
            
#if GOOGLEPLAY_LOGIN           
            ret.playerID = PlayGamesPlatform.Instance.GetUserId();
            ret.displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            ret.email = PlayGamesPlatform.Instance.GetUserEmail();
            ret.token = PlayGamesPlatform.Instance.GetIdToken();
            ret.authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
#endif
            return ret;
        }
        
        public string GetUserId()
        {
            
#if GOOGLEPLAY_LOGIN    
            return PlayGamesPlatform.Instance.GetUserId();
#else
            return null;
#endif
        }
        
        public bool IsAuthenticate()
        {
#if GOOGLEPLAY_LOGIN    
            return PlayGamesPlatform.Instance.IsAuthenticated();
#else
            return false;
#endif
        }

        public void Authenticate(Action<int> callback)
        {
#if GOOGLEPLAY_LOGIN
            PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>
            {
                try
                {
                    callback?.Invoke((int)result);
                }
                catch (Exception e)
                {
                   AccountLog.LogError(e);
                }
            });
#endif
        }
    }
}