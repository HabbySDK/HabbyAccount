using System;
using System.Collections.Generic;
using UnityEngine;
using Habby.Account;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Habby.Account.Data;
using Habby.Tool;

namespace Habby.Account
{

    public class AccountSetting
    {
        public string serverUrl;
        public string appLanguage;
        public string netVersion;
    }
    public class HabbyAccountManager : MonoBehaviour, IClientData
    {
        public const string SDKVersion = "1.0";
        
        private static HabbyAccountManager sInstance = null;
        public static HabbyAccountManager Instance
        {
            get
            {
                if(sInstance == null)
                {
                    GameObject tobj = new GameObject("HabbySDKLauncher");
                    GameObject.DontDestroyOnLoad(tobj);

                    sInstance = tobj.AddComponent<HabbyAccountManager>();
                    sInstance.Init();
                }
                return sInstance;
            }
        }

        public static Func<string> deviceIdDelgate
        {
            get
            {
                return HabbyAccountBase.deviceIdDelgate;
            }

            set
            {
                HabbyAccountBase.deviceIdDelgate = value;
            }
        }
        

        public static void Init(AccountSetting pSetting, HabbyAccountBase pAccount, bool useDefaultOnFail = true)
        {
            Instance._setting = pSetting;
            Instance.useDefaultLoginOnFail = useDefaultOnFail;
            SetAccount(pAccount);
            Instance.InitClientData();
            
            AccountHttpManager.Instance.AddCustomHeader("ClientData",Instance.customClientData);
        }
        
        void InitClientData()
        {
            var tfields = new Dictionary<string, object>()
            {
                {"deviceId", SystemInfo.deviceUniqueIdentifier},
                {"appVersion", Application.version},
                {"osVersion", SystemInfo.operatingSystem},
                {"systemLanguage", Application.systemLanguage.ToString()},
                {"appBundle", Application.identifier},
                {"deviceModel", UnityEngine.SystemInfo.deviceModel},
            };

            SetClientFields(tfields);
        }
        
        public Dictionary<string, object> customClientData { get; private set; } = new Dictionary<string, object>();
        public void SetClientFields(Dictionary<string, object> pFields)
        {
            if (pFields == null) return;
            foreach (var item in pFields)
            {
                if (customClientData.ContainsKey(item.Key))
                {
                    customClientData[item.Key] = item.Value;
                }
                else
                {
                    customClientData.Add(item.Key, item.Value);
                }
            }
        }

        public static void SetAccount(HabbyAccountBase pAccount)
        {
            Instance._account = pAccount;
            Instance.setUPAccount = pAccount;
        }

        void DefaultLogin(Action<LoginResult> callBack)
        {
            AccountLog.Log("Change Default device Login.");
            deviceAccount.Login((code, data, msg) =>
            {
                if (code == 0)
                {
                    AccountLog.Log($"DefaultAccount {deviceAccount.GetType().Name} Login success.");
                    OnLoginAccount(deviceAccount);
                }
                        
                callBack?.Invoke(deviceAccount.statuCode);
            });
        }

        public static void Login(Action<LoginResult> callBack)
        {
            var curAccount = Instance.setUPAccount;
            curAccount.Login((code, data, msg) =>
            {
                if ((curAccount.statuCode == LoginResult.ChannelLoginFail || curAccount.statuCode == LoginResult.GetChannelTokenFail) && Instance.useDefaultLoginOnFail)
                {
                    Instance.DefaultLogin(callBack);
                }
                else
                {
                    if (code == 0)
                    {
                        AccountLog.Log($"CurAccount {curAccount.GetType().Name} Login success.");
                        Instance.OnLoginAccount(curAccount);
                    }
                    
                    callBack?.Invoke(curAccount.statuCode);
                }
                
            });
        }

        public static void GetSignatureString(System.Action<string> complete)
        {
            account.GetSignatureString(complete);
        }

        public static void RefreshToken(HabbyAccountEvent<TokenData> callBack)
        {
            account.RefreshAccessToken(callBack);
        }
        
        public static bool GetAccessToken(Action<string> callBack)
        {
            return account.GetAccessToken(callBack);
        }

        private HabbyAccountBase setUPAccount;
        
        private HabbyAccountBase _account;
        public static IHabbyAccount account => Instance._account;
        
        private HabbyAccountBase deviceAccount;

        public static string ServerUrl => Instance._setting.serverUrl;
        
        
        AccountSetting _setting;
        public static AccountSetting Setting => Instance._setting;

        private string curLoginPlayerId = null;

        private bool useDefaultLoginOnFail = false;
        
        private bool inited = false;

        void Init()
        {
            if(inited) return;
            inited = true;
            
            deviceAccount = new HabbyAccount_Default();
        }
        
        void OnLoginAccount(HabbyAccountBase pAccount)
        {
            _account = pAccount;
            curLoginPlayerId = pAccount.accountData.playerId;
            lastSendPlayerChangedKey = null;
        }

        #region update
        
        private void Update()
        {
            _account?.Update();

            UpdateCheck();
        }
        
        private float checkPlayerintervalTimer = 0;
        void UpdateCheck()
        {
            if(checkPlayerintervalTimer > Time.realtimeSinceStartup) return;
            checkPlayerintervalTimer = Time.realtimeSinceStartup + 2;

            UpdateCheckPlayerChanged();
        }
        
        void UpdateCheckPlayerChanged()
        {
            if(checkPlayerintervalTimer < Time.realtimeSinceStartup) return;
            checkPlayerintervalTimer = Time.realtimeSinceStartup + 2;
            if (_account == null) return;
            
            if (_account.statuCode != LoginResult.OK) return;

            var tchannelId = _account.GetChannelPlayerId();
            if (string.Equals(curLoginPlayerId, tchannelId)) return;
            
            string tkey = $"{curLoginPlayerId}-{tchannelId}";

            if (lastSendPlayerChangedKey == null || string.Equals(lastSendPlayerChangedKey, tkey))
            {
                lastSendPlayerChangedKey = tkey;
                return;
            }

            try
            {
                lastSendPlayerChangedKey = tkey;
                OnPlayerChangedEvent?.Invoke();
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
            
        }

        private string lastSendPlayerChangedKey = null;
        public static event System.Action OnPlayerChangedEvent = null;
        #endregion


        public void OnRecAccountMessage(string json)
        {
            AccountLog.LogFormat("[OnRecAccountMessage]:{0}", json);
            if(string.IsNullOrEmpty(json)) return;

            try
            {
                RecMessageItem tmsgobj = DataConvert.FromJson<RecMessageItem>(json);
                if(tmsgobj == null) return;

                if (string.IsNullOrEmpty(tmsgobj.eventName) || tmsgobj.seqId == -1)
                {
                    AccountLog.LogFormat("[OnRecAccountMessage]:event:{0},sid{1}", tmsgobj.eventName,tmsgobj.seqId);
                    return;
                }
                
                var tchannelAccount = Instance.setUPAccount;
                if (tchannelAccount != null && tchannelAccount is IReciveMessage)
                {
                    ((IReciveMessage)tchannelAccount).OnReciveMessage(tmsgobj.eventName,tmsgobj.seqId,json);
                }
            }
            catch (System.Exception e)
            {
                AccountLog.LogError($"OnRecAccountMessage error = {e}");
            }
            
            
        }

    }

    public class RecMessageItem
    {
        public string eventName;
        public int seqId = -1;
    }
}