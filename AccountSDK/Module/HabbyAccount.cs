using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Habby.Account.Data;
using Habby.Account.Process;
using Habby.Account.Sender;
using Habby.Account.State;

using Habby.Tool;
using UnityEngine;
using Habby.Tool.IO;
using UnityEngine.XR;

namespace Habby.Account
{
    
    public abstract class HabbyAccountBase : IHabbyAccount, IReciveMessage
    {
        public static Func<string> deviceIdDelgate = null;
        
        public static string GetMD5(string pStr)
        {
            byte[] data = Encoding.UTF8.GetBytes(pStr);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] OutBytes = md5.ComputeHash(data);
            StringBuilder tstr = new StringBuilder();
            for (int i = 0; i < OutBytes.Length; i++)
            {
                tstr.Append(OutBytes[i].ToString("x2"));
            }
            return tstr.ToString();
        }
        
        const string accountFile = "account.data";
        
        protected static int _seqIndex = 1;
        protected static int SeqId
        {
            get { return _seqIndex++; }
        }

        private string _accountPathFile;
        public string accountPathFile
        {
            get
            {
                if (_accountPathFile == null)
                {
                    string tname = GetMD5(GetType().Name);
                    _accountPathFile = $"{Application.persistentDataPath}/{tname}_{accountFile}";
                }

                return _accountPathFile;
            }
        }

        #region interfaceField
        public HabbyAccountData accountData { get; protected set; } = new HabbyAccountData();
        public LoginResult statuCode { get; protected set; } = LoginResult.notLogedIn;
        #endregion

        #region member
        protected Dictionary<string, System.Action<int, JToken>> eventMap =
            new Dictionary<string, Action<int, JToken>>();

        protected Dictionary<int, ISender> senderMap = new Dictionary<int, ISender>();

        protected IAccountState curState { get; private set; }
        protected Dictionary<StateType, IAccountState> stateMap = new Dictionary<StateType, IAccountState>();
        
        protected ProcessSequen loginSequen;
        protected ProcessSequen noSigCheckloginSequen;
        #endregion

        #region init
        public HabbyAccountBase()
        {
            eventMap.Add("OnIdentityVerificationSignature", OnIdentityVerificationSignature);
            eventMap.Add("OnLogin", OnLogin);

            stateMap.Add(StateType.loginState, new LoginState(this));
            stateMap.Add(StateType.normalState, new NormalState(this));

            curState = stateMap[StateType.loginState];

            InitLoginProcess();
            InitGetSigDataProcess();
            LoadData();
        }

        public void LoadData()
        {
            if (!File.Exists(accountPathFile)) return;
            try
            {
                var treader = new AESReader(accountPathFile);
                var tjsong = treader.ReadString();
                treader.Close();

                DataConvert.MergeFromJson(accountData, tjsong);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }

        public void SaveData()
        {
            try
            {
                var twriter = new AESWriter(accountPathFile);
                string tdatastr = DataConvert.ToJson(accountData);
                twriter.WriteString(tdatastr);

                twriter.Close();

                AccountLog.LogFormat("Save token.path = {0}", accountPathFile);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }

        #endregion
        
        #region initProcess

        virtual protected ProcessSequen GetChannelLoginProcess()
        {
            return null;
        }

        virtual protected ProcessSigToServer GetSigToServerProcess()
        {
            if (this is ILogin)
            {
                return new ProcessSigToServer((ILogin)this);
            }

            return null;
        }
        
        virtual protected ProcessSequen GetSigStringProcess()
        {
            return null;
        }
        
        private void InitLoginProcess()
        {
            loginSequen = new ProcessSequen();
            
            var channelLoginSequen = GetChannelLoginProcess();
            var sigToServerProcess = GetSigToServerProcess();

            if (channelLoginSequen != null)
            {
                channelLoginSequen.OnDoneEvent += (code, data, err) =>
                {
                    if (code != 0)
                    {
                        statuCode = LoginResult.ChannelLoginFail;
                    }
                };
                loginSequen.Add(channelLoginSequen);
            }

            if (sigToServerProcess != null)
            {
                sigToServerProcess.OnDoneEvent += (code, data, err) =>
                {
                    if (code != 0)
                    {
                        statuCode = LoginResult.ServerFail;
                    }
                };
                loginSequen.Add(sigToServerProcess);
            }

            loginSequen.onComplete += (sender) =>
            {
                AccountLog.Log("Login process end.");
            };
        }

        void InitGetSigDataProcess()
        {
            noSigCheckloginSequen = new ProcessSequen();
            
            var channelLoginSequen = GetSigStringProcess();
            if (channelLoginSequen != null)
            {
                channelLoginSequen.OnDoneEvent += (code, data, err) =>
                {
                };
                noSigCheckloginSequen.Add(channelLoginSequen);
            }
            
            noSigCheckloginSequen.onComplete += (sender) =>
            {
                AccountLog.Log("noSigChecklogin process end.");
            };
        }

        #endregion

        #region memberMethod

        protected void ChangeState(StateType pState)
        {
            curState = stateMap[pState];
        }

        protected void OnLoginSuccess()
        {
            statuCode = LoginResult.OK;
            ChangeState(StateType.normalState);
        }

        #endregion
        
        #region interfaceMethod

        public void Login(HabbyAccountEvent<HabbyAccountData> complete)
        {
            if (curState.type != StateType.loginState)
            {
                ChangeState(StateType.loginState);
            }

            curState.Login(complete);
        }

        public void RefreshAccessToken(HabbyAccountEvent<TokenData> complete)
        {
            curState.RefreshAccessToken(complete);
        }
        
        public void GetSignatureString(System.Action<string> complete)
        {
            var tseq = noSigCheckloginSequen;
            tseq.Start(() =>
            {
                if (tseq.code == 0)
                {
                    complete?.Invoke(GetChannelJsonToken());
                }
                else
                {
                    complete?.Invoke(null);
                }
            });
        }

        abstract public string GetChannelJsonToken();
        public abstract bool IsAuthenticate();
        public abstract string GetChannelPlayerId();

        public bool IsLoginTimeOut()
        {
            long tnow = DataUtile.GetTimestamp(System.DateTime.UtcNow) / 1000;
            bool ret = accountData?.refreshTokenExpiresAt < tnow;
            // AccountLog.LogFormat("IsLoginTimeOut:cur = {0},refreshTokenExpiresAt = {1}, ret = {2}", tnow,
            //     accountData?.refreshTokenExpiresAt, ret);
            return ret;
        }

        public TimeOutType IsWillTimeOut()
        {
            if (accountData == null) return TimeOutType.RefreshTimeOut;
            long tnow = DataUtile.GetTimestamp(System.DateTime.UtcNow) / 1000 + 60;
            if (accountData.refreshTokenExpiresAt < tnow)
            {
                return TimeOutType.RefreshTimeOut;
            }

            if (accountData.accessTokenExpiresAt < tnow)
            {
                return TimeOutType.accessTimeOut;
            }

            return TimeOutType.normal;
        }

        public bool IsLoggedOn()
        {
            if (!IsAuthenticate() || IsLoginTimeOut() || string.IsNullOrEmpty(accountData?.playerId))
            {
                return false;
            }

            return true;
        }

        public string GetDeviceId()
        {
            if (deviceIdDelgate != null)
            {
                return deviceIdDelgate();
            }
            else
            {
                return SystemInfo.deviceUniqueIdentifier;
            }
        }

        public bool GetAccessToken(Action<string> callBack)
        {
            var tout = IsWillTimeOut();
            switch (tout)
            {
                case TimeOutType.normal:
                {
                    CallDelgate(callBack, 0, accountData?.accessToken);
                }
                    return true;
                case TimeOutType.accessTimeOut:
                {
                    RefreshAccessToken((code, data, err) => { CallDelgate(callBack, code, data?.accessToken); });
                }
                    return false;
                case TimeOutType.RefreshTimeOut:
                {
                    Login((code, data, err) => { CallDelgate(callBack, code, data?.accessToken); });
                }
                    return false;
            }

            void CallDelgate(Action<string> del, int code, string pToken)
            {
                try
                {
                    if (code == 0)
                    {
                        callBack?.Invoke(pToken);
                    }
                    else
                    {
                        callBack?.Invoke(null);
                    }
                }
                catch (Exception e)
                {
                    AccountLog.LogError(e);
                }
            }

            return false;
        }

        public void SetAccountToken(TokenData response)
        {
            accountData.accessToken = response.accessToken;
            accountData.accessTokenExpiresAt = response.accessTokenExpiresAt;
            accountData.refreshToken = response.refreshToken;
            accountData.refreshTokenExpiresAt = response.refreshTokenExpiresAt;
        }

        #endregion

        #region recMsg

        protected ISender DequeueSender(int seq)
        {
            if (!senderMap.ContainsKey(seq))
            {
                AccountLog.LogError($"GetSender failed. seq = {seq}");
                return null;
            }
            var ret = senderMap[seq];
            senderMap.Remove(seq);
            return ret;
        }

        protected void EnqueueSender(ISender sender)
        {
            senderMap.Add(sender.seqId, sender);
        }


        void IReciveMessage.OnReciveMessage(string eventName, int sid, string jsonData)
        {
            try
            {
                CallEvent(eventName, sid, jsonData);

                var tsender = DequeueSender(sid);
                if (tsender != null)
                {
                    tsender.LoadDataFromJson(jsonData);
                    tsender.CallEvent();
                }
            }
            catch (System.Exception e)
            {
                AccountLog.LogError($"IReciveMessage.OnReciveMessage error = {e}");
            }
        }

        protected void CallEvent(string eventName, int sid, string jsonData)
        {
            if (eventName == null) return;
            if (!eventMap.ContainsKey(eventName)) return;
            try
            {
                eventMap[eventName].Invoke(sid, jsonData);
            }
            catch (Exception e)
            {
                AccountLog.LogError($"CallEvent error = {e}");
            }
        }

        protected virtual void OnIdentityVerificationSignature(int sid, JToken jsonData)
        {
        }

        protected virtual void OnLogin(int sid, JToken jsonData)
        {
        }

        #endregion

        #region updateMethod

        public void Update()
        {
            curState?.Update();
        }

        #endregion
    }
}