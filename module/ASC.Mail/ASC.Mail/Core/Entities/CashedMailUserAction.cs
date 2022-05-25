using System.Collections.Generic;

namespace ASC.Mail.Core.Entities
{
    public class CashedMailUserAction
    {
        public string UserName { get; set; }
        public int Tenant { get; set; }
        public List<int> Uds { get; set; }
        public MailUserAction Action { get; set; }
        public int Destination { get; set; }
    }

    public enum MailUserAction
    {
        Nothing,
        SetAsRead,
        SetAsUnread,
        SetAsImportant,
        SetAsNotImpotant,
        SetAsDeleted,
        StartImapClient,
        MoveTo,
        ReceiptStatusChanged
    }
}
