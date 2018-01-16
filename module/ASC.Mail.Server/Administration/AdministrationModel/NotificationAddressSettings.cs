using System;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Mail.Server.Administration.ServerModel
{
    [Serializable]
    [DataContract]
    public class NotificationAddressSettings : BaseSettings<NotificationAddressSettings>
    {
        [DataMember]
        public string NotificationAddress { get; set; }

        public override ISettings GetDefault()
        {
            return new NotificationAddressSettings {NotificationAddress = String.Empty};
        }

        public override Guid ID
        {
            get { return new Guid("{C440A7BE-A336-4071-A57E-5E89725E1CF8}"); }
        }
    }
}
