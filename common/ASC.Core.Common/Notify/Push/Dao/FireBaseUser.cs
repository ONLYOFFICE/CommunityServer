using System;

namespace ASC.Core.Common.Notify.FireBase.Dao
{
    public class FireBaseUser
    {
        public Guid UserId { get; set; }
        public int TenantId { get; set; }
        public string FirebaseDeviceToken { get; set; }
        public string Application { get; set; }
        public bool? IsSubscribed { get; set; }
    }
}
