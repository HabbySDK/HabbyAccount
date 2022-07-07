

using System;
using Habby.Account.Data;
using UnityEngine;

namespace Habby.Account.Process
{

    public class ProcessRefreshToken: ProcessBase<TokenData>
    {
        private HabbyAccountBase account;

        public ProcessRefreshToken(HabbyAccountBase pobj)
        {
            account = pobj;
        }
        
        protected override void OnStart()
        {
            try
            {
                RegToServer();
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }

        void RegToServer()
        {
            var tpath = new AccountRequestPath("v1/tokens");
            tpath.AddKeyword("action","refresh");

            var treq = new RefreshAccessTokenRequest();
            treq.refreshToken = account.accountData.refreshToken;

            AccountHttpManager.Instance.StartPost<Response<RefreshAccessTokenResponse>>(tpath.GetRequestUrl(),treq,OnResponse);
        }

        void OnResponse(Response<RefreshAccessTokenResponse> response,string emsg,int errorcode)
        {
            try
            {
                if (response != null && response.code == 0)
                {
                    code = response.code;
                    data = response.data?.GetTokenData();
                    errorMsg = response.message;
                }
                else
                {
                    code = errorcode;
                    errorMsg = response?.message ?? emsg;
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