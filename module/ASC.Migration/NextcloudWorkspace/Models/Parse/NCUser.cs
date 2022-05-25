using System.Collections.Generic;

namespace ASC.Migration.NextcloudWorkspace.Models
{
    public class NCUser
    {
        public string Uid { get; set; }
        public NCUserData Data { get; set; }
        public NCAddressbooks Addressbooks { get; set; }
        public List<NCCalendars> Calendars { get; set; }
        public NCStorages Storages { get; set; }
    }

    public class NCUserData
    {
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Twitter { get; set; }
    }
}
