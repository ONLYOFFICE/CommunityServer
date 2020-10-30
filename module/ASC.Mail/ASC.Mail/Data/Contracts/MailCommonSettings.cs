/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Runtime.Serialization;
using ASC.Core.Common.Settings;

namespace ASC.Mail.Data.Contracts
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

        [DataMember(Name = "ReplaceMessageBody")]
        public bool ReplaceMessageBodySetting { get; set; }

        public override ISettings GetDefault()
        {
            return new MailCommonSettings
            {
                EnableConversationsSetting = true,
                AlwaysDisplayImagesSetting = false,
                CacheUnreadMessagesSetting = false, //TODO: Change cache algoritnm and restore it back
                EnableGoNextAfterMoveSetting = false,
                ReplaceMessageBodySetting = false
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

        public static bool ReplaceMessageBody
        {
            set
            {
                var setting = LoadForCurrentUser();
                setting.ReplaceMessageBodySetting = value;
                setting.SaveForCurrentUser();
            }
            get
            {
                return LoadForCurrentUser().ReplaceMessageBodySetting;
            }
        }
    }
}
