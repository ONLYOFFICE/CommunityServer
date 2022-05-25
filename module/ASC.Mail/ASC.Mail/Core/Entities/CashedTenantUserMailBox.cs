using System.Collections.Generic;

namespace ASC.Mail.Core.Entities
{
    public class CachedTenantUserMailBox
    {
        public string UserName { get; set; }
        public int Tenant { get; set; }
        public int MailBoxId { get; set; }
        public int Folder { get; set; }
        public IEnumerable<int> Tags { get; set; }
    }
}
