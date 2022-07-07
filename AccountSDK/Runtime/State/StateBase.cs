using System;
using Habby.Account.Data;

namespace Habby.Account.State
{
    public enum StateType
    {
        none = 0,
        loginState,
        normalState,
    }
    public interface IAccountState
    {
        HabbyAccountBase account { get; }
        StateType type { get; }
        void Update();
        void Login(HabbyAccountEvent<HabbyAccountData> callback);
        void RefreshAccessToken(HabbyAccountEvent<TokenData> callback);
    }
    public abstract class StateBase : IAccountState
    {
        public HabbyAccountBase account { get; }
        public StateType type { get; protected set; }
        public abstract void Update();
        virtual public void Login(HabbyAccountEvent<HabbyAccountData> callback)
        {
            AccountLog.Log("State must implement this method");
            callback?.Invoke(-1,null,null);
        }

        virtual public void RefreshAccessToken(HabbyAccountEvent<TokenData> callback)
        {
            AccountLog.Log("State must implement this method");
            callback?.Invoke(-1,null,null);
        }
        

        public StateBase(HabbyAccountBase pAccount)
        {
            account = pAccount;
        }

    }
}