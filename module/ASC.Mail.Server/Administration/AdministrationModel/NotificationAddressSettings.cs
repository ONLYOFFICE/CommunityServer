using System;
using System.Runtime.Serialization;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Mail.Server.Administration.ServerModel
{
    [Serializable]
    [DataContract]
    public class NotificationAddressSettings : ISettings
    {
        [DataMember]
        public string NotificationAddress { get; set; }

        public ISettings GetDefault()
        {
            return new NotificationAddressSettings {NotificationAddress = String.Empty};
        }

        public Guid ID
        {
            get { return new Guid("{C440A7BE-A336-4071-A57E-5E89725E1CF8}"); }
        }
    }
}
