using Newtonsoft.Json.Linq;
namespace Habby.Account.Sender
{
    public interface ISender
    {
        int seqId { get; }
        void CallEvent();
        void LoadDataFromJson(string json);
    }
}