using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Habby;
using Habby.Account;
using Newtonsoft.Json;

namespace Habby.Account
{
    public class AccountSample : MonoBehaviour
    {
        public Button btn1;
        public Button btn2;
        public Button btn3;
        public Button btn4;
        // Start is called before the first frame update
        void Start()
        {
            Debug.LogError(Application.dataPath);
            InitSDK();

            btn1.onClick.AddListener(() =>
            {
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
            
                
            });
            
            btn2.onClick.AddListener(() =>
            {
                HabbyAccountManager.RefreshToken((code, data,error) =>
                {

                    Debug.Log($"============RefreshToken===========");
                    
                    if (code == 0)
                    {
                        Debug.Log("RefToken-> " + data?.accessToken);
                    }
                });
            });
        
            btn3.onClick.AddListener(() =>
            {
                //If accessToken doesn't time out,  isSuccess = true
                bool isSuccess = HabbyAccountManager.GetAccessToken((token) =>
                {
                    Debug.Log($"============GetAccessToken===========");
                    if (token != null)
                    {
                        Debug.Log("GetAccessToken-> " + token);
                    }
                });
                
                
            });
            
            btn4.onClick.AddListener(() =>
            {
                HabbyAccountManager.GetSignatureString((token) =>
                {
                    Debug.Log($"============GetSignatureString===========");
                    if (token != null)
                    {
                        Debug.Log("GetSignatureString-> " + token);
                    }
                });
            });

            HabbyAccountManager.deviceIdDelgate = () =>
            {
                return SystemInfo.deviceUniqueIdentifier;
            };
            
            Debug.LogError(HabbyAccountManager.deviceIdDelgate());
        }
    
        void InitSDK()
        {
            //testserver "https://test-account-projecta.habby.com"
            //Official server "https://account-projecta.habby.com"
            //pre server "https://pre-account-projecta.habby.com"
            string turl = "https://account.projectasvc.com";
            var tsetting = new AccountSetting()
            {
                serverUrl = turl,
                netVersion = "10",
                appLanguage = "zh_CN",
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
            
            //change setting
            HabbyAccountManager.Setting.appLanguage = "zh_CN";
            //change account
            //HabbyAccountManager.SetAccount(new HabbyAccount_Default());
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

