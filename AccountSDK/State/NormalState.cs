using System;
using Habby.Account.Data;
using Habby.Account.Process;
using Habby.Tool;
using UnityEngine;

namespace Habby.Account.State
{
    public class NormalState : StateBase
    {
        public enum TokenState
        {
            none,
            validTime,
            timeOut,
            refreshing,
            needLogin,
        }
        
        private TokenState tokenState = TokenState.none;
        
        protected ProcessRefreshToken refProcess;
        public NormalState(HabbyAccountBase pAccount) : base(pAccount)
        {
            type = StateType.normalState;
            refProcess = new ProcessRefreshToken(pAccount);
            refProcess.OnDoneEvent += OnRefreshDone;
        }
        
        override public void RefreshAccessToken(HabbyAccountEvent<TokenData> callback)
        {
            var tref = account as IHabbyAccount;
            if (tref != null)
            {
                refProcess.Start(() =>
                {
                    if (refProcess.code == 0)
                    {
                        callback?.Invoke(refProcess.code,refProcess.data as TokenData, refProcess.errorMsg);
                    }
                    else
                    {
                        callback?.Invoke(-1,null,refProcess.errorMsg);
                    }
                    
                });
            }
            else
            {
                callback?.Invoke(-1,null,null);
            }
            
        }
        
        void OnRefreshDone(int code, TokenData data, string error)
        {
            if (code == 0 && data != null)
            {
                account.SetAccountToken(data);
            }
        }

        public override void Update()
        {
            UpdateRefreshTime();
        }

        void UpdateRefreshTime()
        {
            switch (tokenState)
            {
                case TokenState.none:
                    break;
                case TokenState.validTime:
                    ValidTimeUpdate();
                    break;
                case TokenState.timeOut:
                    break;
                case TokenState.refreshing:
                    break;
                case TokenState.needLogin:
                    break;;
            }
        }
        
        void ValidTimeUpdate()
        {
            switch (account.IsWillTimeOut())
            {
                case TimeOutType.normal:
                    break;
                case TimeOutType.accessTimeOut:
                    break;
                case TimeOutType.RefreshTimeOut:
                    break;
            }
        }
        
    }
}