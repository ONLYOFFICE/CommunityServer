using System.Collections.Generic;

namespace ASC.Migration.OwnCloud.Models
{
    public class OCUser
    {
        public string Uid { get; set; }
        public OCUserData Data { get; set; }
        public OCAddressbooks Addressbooks { get; set; }
        public List<OCCalendars> Calendars { get; set; }
        public OCStorages Storages { get; set; }
    }

    public class OCUserData
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }
}
