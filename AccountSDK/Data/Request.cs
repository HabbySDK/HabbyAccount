using System;
using UnityEngine;

namespace Habby.Account.Data
{
    public interface IRequest
    {

    }
    

    public interface IResponse
    {

    }

    public class Response<T>
    {
        public int code;
        public string message;
        public T data;
    }

    public abstract class RequestBase
    {
        public ClientData clientData;
    }
    
    public class ClientData
    {
        public string deviceId;
        public string appVersion;
        public string osVersion;
        public string appLanguage;
        public string systemLanguage;
        public string appBundle;
        public string deviceModel;
        public string protocolVersion;
    }

    public class LoginToServerRequest : RequestBase
    {
        public object verification;
        public string deviceId;
        public string playerId;
        public string teamPlayerId;
        public string gamePlayerId;

        public LoginToServerRequest()
        {
            clientData = new ClientData()
            {
                protocolVersion = HabbyAccountManager.Setting.netVersion,
                appLanguage = HabbyAccountManager.Setting.appLanguage,
                deviceId = SystemInfo.deviceUniqueIdentifier,
                appVersion = Application.version,
                osVersion = SystemInfo.operatingSystem,
                systemLanguage = Application.systemLanguage.ToString(),
                appBundle = Application.identifier,
                deviceModel = UnityEngine.SystemInfo.deviceModel,
            };
        }
    }

    public class LoginToServerResponse : IResponse
    {
        public string accessToken;
        public long accessTokenExpiresAt;
        public string refreshToken;
        public long refreshTokenExpiresAt;

        public TokenData GetTokenData()
        {
            var tdata = new TokenData();
            tdata.accessToken = accessToken;
            tdata.accessTokenExpiresAt = accessTokenExpiresAt;
            tdata.refreshToken = refreshToken;
            tdata.refreshTokenExpiresAt = refreshTokenExpiresAt;

            return tdata;
        }
    }

    public class RefreshAccessTokenRequest : RequestBase
    {
        public string refreshToken;
    }
    
    public class RefreshAccessTokenResponse : LoginToServerResponse
    {
    }
    
    public class TokenData
    {
        public string accessToken;
        public long accessTokenExpiresAt;
        public string refreshToken;
        public long refreshTokenExpiresAt;
    }
}

