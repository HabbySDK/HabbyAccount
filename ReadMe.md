# HabbyAccount


## Initialize

```c#
    using Habby.Account;


    string turl = "https://test-account-projecta.habby.com";
    var tsetting = new AccountSetting()
    {
        serverUrl = turl, // server url
        netVersion = "10", //If the version is not different, enter 0
        appLanguage = "zh_CN",// language
    };
    #if UNITY_EDITOR
            HabbyAccountManager.Init(tsetting,new HabbyAccount_Default());
    #else
    
        #if UNITY_IOS
                HabbyAccountManager.Init(tsetting,new HabbyAccount_GameCenter());
        #elif UNITY_ANDROID
                HabbyAccountManager.Init(tsetting,new HabbyAccount_GooglePlay());
        #else
                HabbyAccountManager.Init(tsetting,new HabbyAccount_Default());
        #endif
    
    #endif
    
```
    


## Change Setting

```c#
    //change setting
    HabbyAccountManager.Setting.appLanguage = "zh_CN";
```

## API

- **Login**
```c#
        //If the login fails, the HabbyAccount_Default login is used
        HabbyAccountManager.Login((code) =>
        {
            Debug.Log($"============Login===========");
                Debug.LogFormat("code:{0}",code);

            if (code == LoginResult.OK)
            {
                var accountData = HabbyAccountManager.account.accountData;
                Debug.Log(accountData.playerId);//channel id
                Debug.Log(accountData.nickName);//nickname
                Debug.Log(accountData.email);//email 
                Debug.Log(accountData.accessToken);//token
                Debug.Log(accountData.accessTokenExpiresAt);//token outtime
                Debug.Log(accountData.refreshToken);//refresh accessToken
                Debug.Log(accountData.refreshTokenExpiresAt);//refreshToken outtime
            }
                    
        });
```

- **Refresh Token**

```c#
        HabbyAccountManager.RefreshToken((code, data,error) =>
        {

            Debug.Log($"============RefreshToken===========");
            
            if (code == 0)
            {
                Debug.Log("RefToken-> " + data?.accessToken);
            }
        });
```


- **AccessToken**

```c#
        //If accessToken doesn't time out,  isSuccess = true
        bool isSuccess = HabbyAccountManager.GetAccessToken((token) =>
        {
            Debug.Log($"============GetAccessToken===========");
            if (token != null)
            {
                Debug.Log("GetAccessToken-> " + token);
            }
        });
```

- **Signature String**

```c#
        HabbyAccountManager.GetSignatureString((token) =>
        {
            Debug.Log($"============GetSignatureString===========");
            if (token != null)
            {
                Debug.Log("GetSignatureString-> " + token);
            }
        });
```

- **DeviceID Delgate**

        //User-defined method for obtaining Deviceid
        //default SystemInfo.deviceUniqueIdentifier
        HabbyAccountManager.deviceIdDelgate = () =>
        {
            return SystemInfo.deviceUniqueIdentifier;
        };