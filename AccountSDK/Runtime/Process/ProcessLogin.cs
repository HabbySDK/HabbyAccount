using System;
using Habby.Account.Data;
using UnityEngine;

namespace Habby.Account.Process
{
    public class ProcessLogin : ProcessBase<AccountDataBase>
    {
        private ILogin account;

        public ProcessLogin(ILogin pAccount)
        {
            account = pAccount;
        }
        protected override void OnStart()
        {
            try
            {
                account.StartLogin(OnRequestComplete);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }
        void OnRequestComplete(int pCode, AccountDataBase pData,string err)
        {
            try
            {
                code = pCode;
                data = pData;
                account.channelData.statusCode = code;
                if (code != 0)
                {
                    errorMsg = $"errorCode = {code}, error = {pData?.error}";
                }
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
            OnEnd();
        }
    }
}