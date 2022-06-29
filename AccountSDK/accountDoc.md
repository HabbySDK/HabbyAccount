
**AccountSDK**


- **Init SDK**

  - Example Scene

    `HabbySDK/HabbyAccount/Example/testsc.unity`

    `HabbySDK/HabbyAccount/Example/AccountSample.cs`

    

  - Using namespace

    using Habby.Account;

  - Init
    
    - DeviceId
    
      `HabbyAccountManager.Init(serverurl,new HabbyAccount_Default());`
      
    - GooglePlay

      `HabbyAccountManager.Init(serverurl,new HabbyAccount_GooglePlay());`

    - Game Center

      `HabbyAccountManager.Init(serverurl,new HabbyAccount_GameCenter());`

  - Login to server

    ```
      HabbyAccountManager.Login((code, data,error) =>
      {
          if (code == 0)
          {
              var accountData = data;
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

  - Get the user ID from GameServer via accessToken and refreshToken
    
    ```
      //your code
      var accountData = HabbyAccountManager.account.accountData;
      accountData.accessToken
      accountData.refreshToken
    ```


- **API**

  - Change Account Object

    `HabbyAccountManager.SetAccount(new HabbyAccount_Default());`

  - Login
  
    ```
        HabbyAccountManager.Login((code, data,error) =>
        {
            if (code == 0)
            {
                var accountData = data;

            }
            
        });

    ```

  - Refresh Token

    ```
        HabbyAccountManager.RefreshToken((code, data,error) =>
        {
            if (code == 0)
            {
                Debug.Log("RefToken-> " + data?.accessToken);
                Debug.Log("RefToken-> " + data?.refreshToken);
            }
        });

    ```

  - GetAccessToken

    ```
        //If accessToken doesn't time out,  isSuccess = true
        // string token
        bool isSuccess = HabbyAccountManager.GetAccessToken((token) =>
        {
            Debug.Log($"============GetAccessToken===========");
            if (token != null)
            {
                Debug.Log("GetAccessToken-> " + token);
            }
            
        });

    ```

- **Event**

  - Account event
  
    `public delegate void HabbyAccountEvent<T>(int code, T data, string error);`

- **Class**

  - HabbyAccountData
    - channel id
  
      `public string playerId;`

    - channel teamPlayerid 
  
      `public string teamPlayerId;`

    - channel nickName

      `public string nickName`

    - channel email

      `public string email`

    - accessToken

      `public string accessToken`

    - refreshToken

      `public string refreshToken`
  - TokenData
  
    - accessToken

      `public string accessToken`

    - refreshToken

      `public string refreshToken`