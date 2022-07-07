
using System;
using Habby.Account.Data;
using Habby.Account.Process;
using Habby.Account.State;
using Habby.Account.ThirdPartInterface;
using Habby.Tool;

namespace Habby.Account
{

    public class HabbyAccount_GooglePlay : HabbyAccountBase, ILogin, IGetToken
    {
        public static GMSInterface gmsInterface = new GMSInterface();
        
        public ChannelDataBase channelData { get; private set; } = new AccountDataGooglePlay();
        AccountDataGooglePlay channelInfo => (AccountDataGooglePlay)channelData;

        public string channelName { get; protected set; } = "gms";
        public HabbyAccount_GooglePlay() : base()
        {
            gmsInterface.Init();
        }

        override protected ProcessSequen GetChannelLoginProcess()
        {
            var ret = new ProcessSequen();
            var tlogingc = new ProcessLogin(this);
            ret.Add(tlogingc);
            return ret;
        }
        
        override protected ProcessSequen GetSigStringProcess()
        {
            var ret = new ProcessSequen();
            var tlogingc = new ProcessLogin(this);
            ret.Add(tlogingc);
            return ret;
        }

        void ILogin.StartLoginProcess(HabbyAccountEvent<HabbyAccountData> complete)
        {
            if (!IsLoginTimeOut() && IsAuthenticate())
            {
                var tuserid = gmsInterface.GetUserId();
                if (!string.IsNullOrEmpty(accountData.playerId) && string.Equals(tuserid, accountData.playerId))
                {
                    AccountLog.LogFormat("allready to login. id= {0}", accountData.playerId);
                    OnLoginSuccess();
                    complete?.Invoke(0, accountData, null);
                    return;
                }
            }

            loginSequen.Start(() =>
            {
                if (loginSequen.code == 0)
                {
                    accountData.playerId = channelInfo.playerID;
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

                AccountLog.Log("Login end.code =" + loginSequen.code);

            });
        }
        
        protected void SetAccountData()
        {
            var tinfo = gmsInterface.GetChannelInfo();
            channelInfo.playerID = tinfo.playerID;
            channelInfo.displayName = tinfo.displayName;
            channelInfo.email = tinfo.email;
            channelInfo.token = tinfo.token;
            channelInfo.authCode = tinfo.authCode;
            
            AccountLog.LogFormat("Login success. userName:{0}, id={1}, email:{2}", channelData.displayName, channelData.playerID, channelData.email);
            AccountLog.LogFormat("token={0}", ((AccountDataGooglePlay)channelData).token);
            AccountLog.LogFormat("AuthCode={0}", ((AccountDataGooglePlay)channelData).authCode);
        }

        public override bool IsAuthenticate()
        {
            return gmsInterface.IsAuthenticate();
        }

        void ILogin.StartLogin(HabbyAccountEvent<AccountDataBase> complete)
        {
            AccountLog.Log($"{this.GetType().Name}:StartLogin");
            if (IsAuthenticate())
            {
                AccountLog.LogFormat("StartLogin; IsAuthenticate={0}", IsAuthenticate());
                SetAccountData();

                var tdata = new AccountDataBase();
                tdata.playerID = channelInfo.playerID;
                tdata.displayName = channelInfo.displayName;
                tdata.email = channelInfo.email;
                complete?.Invoke(0, tdata, null);
                return;
            }

            gmsInterface.Authenticate((result) =>
            {

                if (result == 0)
                {
                    try
                    {
                        SetAccountData();

                        var tdata = new AccountDataBase();
                        tdata.playerID = channelInfo.playerID;
                        tdata.displayName = channelInfo.displayName;
                        tdata.email = channelInfo.email;

                        complete?.Invoke((int)result, tdata, null);
                    }
                    catch (Exception e)
                    {
                        AccountLog.Log("log error." + e.ToString());
                    }

                }
                else
                {
                    AccountLog.Log("Login failed. result = " + result);
                    complete?.Invoke((int)result, null, null);
                }

            });
        }

        void IGetToken.GetToken(HabbyAccountEvent<AccountDataToken> complete)
        {

        }

        object ILogin.GetRequest()
        {
            var ret = new LoginToServerRequest();

            ret.verification = channelData.GetToken();
            ret.deviceId = GetDeviceId();
            ret.playerId = channelInfo.playerID;

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
        
        override public string GetChannelPlayerId()
        {
            if (IsAuthenticate())
            {
                return gmsInterface.GetUserId();
            }
            return null;
        }
    }

}
