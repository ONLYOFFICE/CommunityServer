using System;
using System.Runtime.Serialization;

namespace ASC.Api.Mail.DataContracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "Folder")]
    public class MailFolderData
    {
        [DataMember(IsRequired = true, Name = "id")]
        public int Id { get; set; }

        [DataMember(IsRequired = true, Name = "unread")]
        public int UnreadCount { get; set; }

        [DataMember(IsRequired = true, Name = "total_count")]
        public int TotalCount { get; set; }

        [DataMember(IsRequired = true, Name = "time_modified")]
        public DateTime TimeModified { get; set; }
    }
}
