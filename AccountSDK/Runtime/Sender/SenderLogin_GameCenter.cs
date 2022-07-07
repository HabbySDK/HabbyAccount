using System;
using Newtonsoft.Json.Linq;
using Habby.Account;
using Habby.Account.Data;
using Habby.Tool;

namespace Habby.Account.Sender
{
    public class SenderLogin_GameCenter : ISender
    {
        public int seqId { get; private set; }
        public AccountDataBase data { get; private set; }

        private HabbyAccountEvent<AccountDataBase> onComplete;

        public SenderLogin_GameCenter(int seq, HabbyAccountEvent<AccountDataBase> completeEvent)
        {
            seqId = seq;
            onComplete = completeEvent;
        }

        public void CallEvent()
        {
            try
            {
                if(data != null)
                    onComplete.Invoke(data.code, data,data.error);
                else
                    onComplete.Invoke(-1, null,null);
            }
            catch (Exception e)
            {
                AccountLog.LogError(e);
            }
        }

        public void LoadDataFromJson(string json)
        {
            data = DataConvert.FromJson<AccountDataBase>(json);
        }
    }
}