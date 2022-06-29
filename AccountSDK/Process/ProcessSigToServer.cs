using System;
using Habby.Account.Data;
using UnityEngine;

namespace Habby.Account.Process
{

    public class ProcessSigToServer: ProcessBase<LoginToServerResponse>
    {
        private ILogin account;

        public ProcessSigToServer(ILogin pobj)
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
            var tpath = new AccountRequestPath($"v1/identityProviders/{account.channelName}");
            tpath.AddKeyword("action","login");

            var treq = account.GetRequest();

            AccountHttpManager.Instance.StartPost<Response<LoginToServerResponse>>(tpath.GetRequestUrl(),treq,OnResponse);
        }

        void OnResponse(Response<LoginToServerResponse> response,string emsg,int errorcode)
        {
            try
            {
                if (response != null && response.code == 0)
                {

                    account?.SetTokenData(response.data.GetTokenData());
                    
                    code = response.code;
                    data = response.data;
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