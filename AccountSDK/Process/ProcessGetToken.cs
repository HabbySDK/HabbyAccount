using System;
using Habby.Account.Data;
using UnityEngine;

namespace Habby.Account.Process
{
    public class ProcessGetToken : ProcessBase<AccountDataToken>
    {
        private IGetToken account;

        public ProcessGetToken(IGetToken pAccount)
        {
            account = pAccount;
        }
        protected override void OnStart()
        {
            try
            {
                account.GetToken(OnRequestComplete);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }
        void OnRequestComplete(int pCode, AccountDataToken pData,string pMsg)
        {
            try
            {
                code = pCode;
                data = pData;
                if (code != 0)
                {
                    errorMsg = $"errorCode = {code}, error = {pData?.error}";
                }

                data = pData;
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
            OnEnd();
        }
    }
}