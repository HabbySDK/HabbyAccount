
#if UNITY_IOS && !IOS_GAMECENTER
#define IOS_GAMECENTER
#endif

using System;
using System.Runtime.InteropServices;
using Habby.Account.Sender;
using Habby.Tool;
using Newtonsoft.Json.Linq;
using Habby.Account.Data;
using Habby.Account.Process;
using Habby.Account.State;

namespace Habby.Account
{

    public class HabbyAccount_GameCenter : HabbyAccountBase, ILogin, IGetToken
    {
#if IOS_GAMECENTER
        [DllImport("__Internal")]
        public static extern void _getIdentityVerificationSignature(int seqId);

        [DllImport("__Internal")]
        public static extern void _loginGameCenter(int seqId);

        [DllImport("__Internal")]
        public static extern bool _IsAuthenticate();

        [DllImport("__Internal")]
        public static extern string _GetLocalPlayerData();

        [DllImport("__Internal")]
        public static extern string _GetUUID(string unityuuid);

#endif
        
        public ChannelDataBase channelData { get; private set; } = new AccountDataGameCenter();
        AccountDataGameCenter channelInfo => (AccountDataGameCenter)channelData;
        

        public string channelName { get; protected set; } = "gamecenter";

        public HabbyAccount_GameCenter() : base()
        {
        }
        
        override protected ProcessSequen GetChannelLoginProcess()
        {
            var ret = new ProcessSequen();
            var tlogingc = new ProcessLogin(this);
            var tsig = new ProcessGetToken(this);
            ret.Add(tlogingc);
            ret.Add(tsig);
            return ret;
        }
        
        override protected ProcessSequen GetSigStringProcess()
        {
            var ret = new ProcessSequen();
            var tsig = new ProcessGetToken(this);
            tsig.OnDoneEvent += (code, res, error) =>
            {
                if (code == 0)
                {
                    var data = GetLocalPlayerData();
                    if (data != null)
                    {
                        channelInfo.playerID = data.playerID;
                        channelInfo.teamPlayerID = data.teamPlayerID;
                        channelInfo.gamePlayerID = data.gamePlayerID;
                        channelInfo.displayName = data.displayName;
                        channelInfo.email = data.email;
                    }
                }
            };
            ret.Add(tsig);
            return ret;
        }

        void ILogin.StartLoginProcess(HabbyAccountEvent<HabbyAccountData> complete)
        {

            if (!IsLoginTimeOut() && IsAuthenticate())
            {
                var tuser = GetLocalPlayerData();
                if (!string.IsNullOrEmpty(accountData.playerId) && string.Equals(tuser.playerID, accountData.playerId))
                {
                    AccountLog.LogFormat("allready to login. id= {0}", accountData.playerId);
                    OnLoginSuccess();
                    complete?.Invoke(0, accountData, null);
                    return;
                }
            }
            
            //((IProcess)sigToServer).SetBypass(pType == LoginType.bypassSigServer);

            loginSequen.Start(() =>
            {
                if (loginSequen.code == 0)
                {
                    accountData.playerId = channelInfo.playerID;
                    accountData.teamPlayerId = channelInfo.teamPlayerID;
                    accountData.nickName = channelInfo.displayName;
                    accountData.email = channelInfo.email;
                    
                    OnLoginSuccess();
                    complete?.Invoke(0, accountData, null);
                }
                else
                {
                    complete?.Invoke(loginSequen.code, null, loginSequen.errorMsg);
                }
                SaveData();
            });
        }
        
        public AccountDataBase GetLocalPlayerData()
        {
#if IOS_GAMECENTER
            string tdata = _GetLocalPlayerData();
            if (string.IsNullOrEmpty(tdata)) return null;
            var ret = DataConvert.FromJson<AccountDataBase>(tdata);
            return ret;
#else
            return null;
#endif
            
        }


        public override bool IsAuthenticate()
        {
#if IOS_GAMECENTER
            return _IsAuthenticate();
#else
            return false;
#endif

        }
        
        override public string GetChannelPlayerId()
        {
            if (IsAuthenticate())
            {
                var ret = GetLocalPlayerData();
                return ret?.playerID;
            }
            return null;
        }

        protected override void OnIdentityVerificationSignature(int sid, JToken jsonData)
        {

        }

        protected override void OnLogin(int sid, JToken jsonData)
        {
        }

        void ILogin.StartLogin(HabbyAccountEvent<AccountDataBase> complete)
        {
            AccountLog.Log($"{this.GetType().Name}:StartLogin");
#if IOS_GAMECENTER
            var tsender = new SenderLogin_GameCenter(SeqId, (code, data, error) =>
            {
                if (code == 0 && data != null)
                {
                    channelInfo.playerID = data.playerID;
                    channelInfo.teamPlayerID = data.teamPlayerID;
                    channelInfo.gamePlayerID = data.gamePlayerID;
                    channelInfo.displayName = data.displayName;
                    channelInfo.email = data.email;
                }

                complete?.Invoke(code, data, error);
            });
            this.EnqueueSender(tsender);
            _loginGameCenter(tsender.seqId);
#endif

        }

        void IGetToken.GetToken(HabbyAccountEvent<AccountDataToken> complete)
        {
#if IOS_GAMECENTER
            AccountLog.Log($"{this.GetType().Name}:GetToken");
            var tsender = new SenderSignature_GameCenter(SeqId, (code, pData, err) =>
            {
                if (code == 0 && pData != null)
                {
                    channelInfo.publicKeyURL = pData.publicKeyURL;
                    channelInfo.signature = pData.signature;
                    channelInfo.salt = pData.salt;
                    channelInfo.timestamp = pData.timestamp;
                    channelInfo.methodTag = pData.methodTag;
                }
                complete?.Invoke(code, pData, err);
            });
            this.EnqueueSender(tsender);
            _getIdentityVerificationSignature(tsender.seqId);
#endif

        }

        object ILogin.GetRequest()
        {
            var ret = new LoginToServerRequest();

            ret.verification = channelData.GetToken();
            ret.deviceId = GetDeviceId();
            ret.playerId = channelInfo.playerID;
            ret.teamPlayerId = channelInfo.teamPlayerID;
            ret.gamePlayerId = channelInfo.gamePlayerID;

            return ret;
        }
        
        void ILogin.SetTokenData(TokenData response)
        {
            SetAccountToken(response);
        }
        
        override public string GetChannelJsonToken()
        {
            var tobj = ((ILogin)(this)).GetRequest();
            return DataConvert.ToJson(tobj);
        }

    }


}
