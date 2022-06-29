
using System;
using Habby.Account.Data;
using Habby.Account.Process;
using Habby.Account.State;
using Habby.Tool;
using UnityEngine;

namespace Habby.Account
{
    public class HabbyAccount_Default : HabbyAccountBase, ILogin, IGetToken
    {
        private IProcess sigToServer;
        
        public ChannelDataBase channelData { get; private set; } = new ChannelDataBase();
        public string channelName { get; protected set; } = "device";
        
        public HabbyAccount_Default()
        {
            channelData.accountType = Data.AccountType.deviceId;
        }

        void ILogin.StartLoginProcess(HabbyAccountEvent<HabbyAccountData> complete)
        {

            if (!IsLoginTimeOut() && IsAuthenticate())
            {
                string tid = SystemInfo.deviceUniqueIdentifier;
                if (!string.IsNullOrEmpty(accountData.playerId) && string.Equals(tid, accountData.playerId))
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
        
        public override bool IsAuthenticate()
        {
            return statuCode == LoginResult.OK;
        }

        override public string GetChannelJsonToken()
        {
            var tobj = ((ILogin)(this)).GetRequest();
            return DataConvert.ToJson(tobj);
        }

        override public string GetChannelPlayerId()
        {
            return accountData?.playerId;
        }

        void ILogin.StartLogin(HabbyAccountEvent<AccountDataBase> complete)
        {
            AccountLog.Log("default can Not StartLogin.");
        }

        void IGetToken.GetToken(HabbyAccountEvent<AccountDataToken> complete)
        {
            AccountLog.Log("default can Not GetToken.");
        }
        

        object ILogin.GetRequest()
        {
            var ret = new LoginToServerRequest();

            ret.verification = channelData.GetToken();
            ret.deviceId = GetDeviceId();

            return ret;
        }
        
        void ILogin.SetTokenData(TokenData response)
        {
            SetAccountToken(response);
        }
    }
}