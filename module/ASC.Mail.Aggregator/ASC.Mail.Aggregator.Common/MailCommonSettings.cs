using System;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Mail.Aggregator.Common
{
    [Serializable]
    [DataContract]
    public class MailCommonSettings : BaseSettings<MailCommonSettings>
    {
        [DataMember(Name = "EnableConversations")]
        public bool EnableConversationsSetting { get; set; }

        [DataMember(Name = "AlwaysDisplayImages")]
        public bool AlwaysDisplayImagesSetting { get; set; }

        [DataMember(Name = "CacheUnreadMessages")]
        public bool CacheUnreadMessagesSetting { get; set; }

        [DataMember(Name = "EnableGoNextAfterMove")]
        public bool EnableGoNextAfterMoveSetting { get; set; }

        public override ISettings GetDefault()
        {
            return new MailCommonSettings
            {
                EnableConversationsSetting = true,
                AlwaysDisplayImagesSetting = false,
                CacheUnreadMessagesSetting = true,
                EnableGoNextAfterMoveSetting = false
            };
        }

        public override Guid ID {
            get { return new Guid("{AA4E16A0-B9F5-402A-A71E-9A1EC6E6B57A}"); }
        }

        public static bool EnableConversations
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.EnableConversationsSetting = value;
                setting.SaveForCurrentUser();
            }
            get
            {
                return LoadForCurrentUser().EnableConversationsSetting;
            }
        }

        public static bool AlwaysDisplayImages
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.AlwaysDisplayImagesSetting = value;
                setting.SaveForCurrentUser();
            }
            get
            {
                return LoadForCurrentUser().AlwaysDisplayImagesSetting;
            }
        }

        public static bool CacheUnreadMessages
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.CacheUnreadMessagesSetting = value;
                setting.SaveForCurrentUser();
            }
            get
            {
                return LoadForCurrentUser().CacheUnreadMessagesSetting;
            }
        }

        public static bool GoNextAfterMove
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.EnableGoNextAfterMoveSetting = value;
                setting.SaveForCurrentUser();
            }
            get
            {
                return LoadForCurrentUser().EnableGoNextAfterMoveSetting;
            }
        }
    }
}
