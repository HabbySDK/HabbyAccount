namespace Habby.Account.Data
{
    public enum AccountType
    {
        none = 0,
        deviceId,
        GameCenter,
        GooglePlay,

    }

    public class HabbyAccountData
    {
        public string playerId;//channel id
        public string teamPlayerId;// null or id
        public string nickName;//nickName
        public string email;//email

        public string accessToken;
        public long accessTokenExpiresAt;//accessToken outtime
        public string refreshToken;
        public long refreshTokenExpiresAt;//refreshToken outtime

        public void Copy(HabbyAccountData sor)
        {
            playerId = sor.playerId;
            teamPlayerId = sor.teamPlayerId;
            nickName = sor.nickName;
            email = sor.email;
            accessToken = sor.accessToken;
            accessTokenExpiresAt = sor.accessTokenExpiresAt;
            refreshToken = sor.refreshToken;
            refreshTokenExpiresAt = sor.refreshTokenExpiresAt;
        }
    }

    public class DataBase
    {
        public int code = -1;
        public int seqId;
        public string error;
    }

    public class AccountDataBase : DataBase
    {
        public string playerID;
        public string displayName;
        public string email;
        public string teamPlayerID;
        public string gamePlayerID;
    }

    public class ChannelDataBase
    {
        public AccountType accountType;
        public string playerID;
        public string displayName;
        public string email;
        public int statusCode;

        virtual public object GetToken()
        {
            return null;
        }

    }

    public class AccountDataGooglePlay : ChannelDataBase
    {
        public class Request
        {
            public string authCode;
        }
        public string token;
        public string authCode;

        public AccountDataGooglePlay()
        {
            accountType = Data.AccountType.GooglePlay;
        }
        override public object GetToken()
        {
            var treq = new Request();
            treq.authCode = authCode;

            return treq;
        }

    }

    public class AccountDataGameCenter : ChannelDataBase
    {
        public class Request
        {
            public string publicKeyURL;
            public string signature;
            public string salt;
            public string timestamp;
            public int methodTag;
        }

        public string teamPlayerID;
        public string gamePlayerID;

        public string publicKeyURL;
        public string signature;
        public string salt;
        public string timestamp;
        public int methodTag;

        public AccountDataGameCenter()
        {
            accountType = Data.AccountType.GameCenter;
        }

        override public object GetToken()
        {
            var treq = new Request();
            treq.publicKeyURL = publicKeyURL;
            treq.signature = signature;
            treq.salt = salt;
            treq.timestamp = timestamp;
            treq.methodTag = methodTag;

            return treq;
        }
    }

    public class AccountDataToken : DataBase
    {
        public string publicKeyURL;
        public string signature;
        public string salt;
        public string timestamp;
        public int methodTag;
    }
}