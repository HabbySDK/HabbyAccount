using Habby.Account.Data;
using UnityEngine;

namespace Habby.Account.State
{
    public class LoginState : StateBase
    {
        public LoginState(HabbyAccountBase pAccount) : base(pAccount)
        {
            type = StateType.loginState;
        }
        
        override public void Login(HabbyAccountEvent<HabbyAccountData> callback)
        {
            var tlogin = account as ILogin;
            if (tlogin != null)
            {
                tlogin.StartLoginProcess(callback);
            }
            else
            {
                callback?.Invoke(-1,null,null);
            }
        }

        public override void Update()
        {

        }
    }
}