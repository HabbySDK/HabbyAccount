using System;
using Habby.Account.Data;
using Habby.Account.State;
using Newtonsoft.Json.Linq;

namespace Habby.Account
{
    public delegate void HabbyAccountEvent<T>(int code, T data, string error);
    public enum TimeOutType
    {
        normal = 0,
        accessTimeOut,
        RefreshTimeOut,
    }

    public enum LoginResult
    {
        OK = 0,
        ChannelLoginFail,
        GetChannelTokenFail,
        ServerFail,

        notLogedIn = 99,
    }

    public enum LoginType
    {
        defaultLogin = 0,
        bypassSigServer,
    }
    public interface IHabbyAccount
    {
        HabbyAccountData accountData { get; }
        LoginResult statuCode { get; }

        void Login(HabbyAccountEvent<HabbyAccountData> complete);
        void RefreshAccessToken(HabbyAccountEvent<TokenData> complete);
        void GetSignatureString(System.Action<string> complete);
        string GetChannelJsonToken();
        string GetChannelPlayerId();
        
        bool IsLoginTimeOut();
        TimeOutType IsWillTimeOut();
        bool IsLoggedOn();
        bool IsAuthenticate();

        string GetDeviceId();
        bool GetAccessToken(Action<string> callBack);
    }

    public interface IReciveMessage
    {
        void OnReciveMessage(string eventName, int sid, JToken jsonData);
    }

    public interface ILogin
    {
        ChannelDataBase channelData { get; }
        string channelName { get; }
        void StartLoginProcess(HabbyAccountEvent<HabbyAccountData> complete);
        void StartLogin(HabbyAccountEvent<AccountDataBase> complete);

        object GetRequest();
        void SetTokenData(TokenData response);
    }

    public interface IGetToken
    {
        void GetToken(HabbyAccountEvent<AccountDataToken> complete);
    }
}