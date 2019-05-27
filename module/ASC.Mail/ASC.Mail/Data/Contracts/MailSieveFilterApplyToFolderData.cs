using System;
using System.Runtime.Serialization;
using ASC.Mail.Enums;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "FilterApplyToFolder")]
    public class MailSieveFilterApplyToFolderData
    {
        [DataMember(IsRequired = true, Name = "type")]
        public FolderType Type { get; set; }

        [DataMember(IsRequired = false, Name = "userFolderId")]
        public int? UserFolderId { get; set; }
    }
}
