using System.Runtime.Serialization;
using LiteDB;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    [DataContract]
    public class MailboxData
    {
        [DataMember(Name = "tenant")]
        public int TenantId { get; set; }

        [DataMember(Name = "user")]
        public string UserId { get; set; }

        [DataMember(Name = "id")]
        public int MailboxId { get; set; }

        [DataMember]
        public string EMail { get; set; }

        [DataMember(Name = "imap")]
        public bool Imap { get; set; }

        [DataMember(Name = "is_teamlab")]
        public bool IsTeamlab { get; set; }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "messages_count")]
        public int MessagesCount { get; set; }
    }
}
