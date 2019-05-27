using System;
using System.Runtime.Serialization;
using ASC.Mail.Enums;
using ASC.Mail.Enums.Filter;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    [DataContract(Namespace = "", Name = "FilterOptions")]
    public class MailSieveFilterOptionsData
    {
        public MailSieveFilterOptionsData()
        {
            ApplyTo = new MailSieveFilterOptionsApplyToData();
        }

        [DataMember(Name = "matchMultiConditions")]
        public MatchMultiConditionsType MatchMultiConditions { get; set; }

        [DataMember(Name = "applyTo")]
        public MailSieveFilterOptionsApplyToData ApplyTo { get; set; }

        [DataMember(Name = "ignoreOther")]
        public bool IgnoreOther { get; set; }
    }

    [Serializable]
    [DataContract(Namespace = "", Name = "FilterOptionsApplyTo")]
    public class MailSieveFilterOptionsApplyToData
    {
        public MailSieveFilterOptionsApplyToData()
        {
            Folders = new[] {(int) FolderType.Inbox};
            Mailboxes = new int[] {};
        }

        [DataMember(Name = "folders")]
        public int[] Folders { get; set; }

        [DataMember(Name = "mailboxes")]
        public int[] Mailboxes { get; set; }

        [DataMember(Name = "withAttachments")]
        public ApplyToAttachmentsType WithAttachments { get; set; }
    }
}
