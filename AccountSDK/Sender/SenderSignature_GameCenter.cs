using System;
using Newtonsoft.Json.Linq;
using Habby.Account;
using Habby.Account.Data;
using Newtonsoft.Json;

namespace Habby.Account.Sender
{
    public class SenderSignature_GameCenter : ISender
    {
        public int seqId { get; private set; }
        public AccountDataToken data { get; private set; }

        private HabbyAccountEvent<AccountDataToken> onComplete;

        public SenderSignature_GameCenter(int seq, HabbyAccountEvent<AccountDataToken> completeEvent)
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

        public void LoadDataFromJson(JToken json)
        {
            data = json?.ToObject<AccountDataToken>();
        }
    }
}