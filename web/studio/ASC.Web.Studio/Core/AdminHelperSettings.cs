using System;
using System.Runtime.Serialization;

using ASC.Core.Common.Settings;

namespace ASC.Web.Studio.Core
{
    [Serializable]
    [DataContract]
    public class AdminHelperSettings : BaseSettings<AdminHelperSettings>
    {
        [DataMember(Name = "Viewed")]
        public bool Viewed { get; set; }

        public override Guid ID
        {
            get { return new Guid("{342CBBF7-FE08-4261-AB38-9C6BA8FA22B9}"); }
        }

        public override ISettings GetDefault()
        {
            return new AdminHelperSettings()
            {
                Viewed = false
            };
        }
    }
}