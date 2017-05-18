using System;
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Core.Common.Settings;

namespace ASC.Mail.Aggregator.Common
{
    [Serializable]
    [DataContract]
    public class MailCommonSettings : ISettings
    {
        [DataMember(Name = "EnableConversations")]
        public bool EnableConversationsSetting { get; set; }

        [DataMember(Name = "AlwaysDisplayImages")]
        public bool AlwaysDisplayImagesSetting { get; set; }

        [DataMember(Name = "CacheUnreadMessages")]
        public bool CacheUnreadMessagesSetting { get; set; }

        [DataMember(Name = "EnableGoNextAfterMove")]
        public bool EnableGoNextAfterMoveSetting { get; set; }

        public ISettings GetDefault()
        {
            return new MailCommonSettings
            {
                EnableConversationsSetting = true,
                AlwaysDisplayImagesSetting = false,
                CacheUnreadMessagesSetting = true,
                EnableGoNextAfterMoveSetting = false
            };
        }

        public static MailCommonSettings GetSettings()
        {
            var settings = SettingsManager.Instance.LoadSettingsFor<MailCommonSettings>(SecurityContext.CurrentAccount.ID);

            return settings;
        }

        public Guid ID {
            get { return new Guid("{AA4E16A0-B9F5-402A-A71E-9A1EC6E6B57A}"); }
        }

        public static bool EnableConversations
        {
            set
            {
                var setting = GetSettings();

                setting.EnableConversationsSetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get
            {
                return
                    SettingsManager.Instance.LoadSettingsFor<MailCommonSettings>(SecurityContext.CurrentAccount.ID)
                        .EnableConversationsSetting;
            }
        }

        public static bool AlwaysDisplayImages
        {
            set
            {
                var setting = GetSettings();

                setting.AlwaysDisplayImagesSetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get
            {
                return
                    SettingsManager.Instance.LoadSettingsFor<MailCommonSettings>(SecurityContext.CurrentAccount.ID)
                        .AlwaysDisplayImagesSetting;
            }
        }

        public static bool CacheUnreadMessages
        {
            set
            {
                var setting = GetSettings();

                setting.CacheUnreadMessagesSetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get
            {
                return
                    SettingsManager.Instance.LoadSettingsFor<MailCommonSettings>(SecurityContext.CurrentAccount.ID)
                        .CacheUnreadMessagesSetting;
            }
        }

        public static bool GoNextAfterMove
        {
            set
            {
                var setting = GetSettings();

                setting.EnableGoNextAfterMoveSetting = value;

                SettingsManager.Instance.SaveSettingsFor(setting, SecurityContext.CurrentAccount.ID);
            }
            get
            {
                return
                    SettingsManager.Instance.LoadSettingsFor<MailCommonSettings>(SecurityContext.CurrentAccount.ID)
                        .EnableGoNextAfterMoveSetting;
            }
        }
    }
}
