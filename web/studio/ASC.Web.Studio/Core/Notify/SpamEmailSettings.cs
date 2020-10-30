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

namespace ASC.Web.Studio.Core.Notify
{
    [Serializable]
    [DataContract]
    public class SpamEmailSettings : BaseSettings<SpamEmailSettings>
    {
        [DataMember(Name = "MailsSendedCount")]
        public int MailsSendedCount { get; set; }

        [DataMember(Name = "MailsSendedDate")]
        public DateTime MailsSendedDate { get; set; }

        public override Guid ID
        {
            get { return new Guid("{A9819A62-60AF-48E3-989C-08259772FA57}"); }
        }

        public override ISettings GetDefault()
        {
            return new SpamEmailSettings
                {
                    MailsSendedCount = 0,
                    MailsSendedDate = DateTime.UtcNow.AddDays(-2)
                };
        }

        public static int MailsSended
        {
            get { return GetCount(); }
            set
            {
                var setting = Load();
                setting.MailsSendedDate = DateTime.UtcNow.Date;
                setting.MailsSendedCount = value;
                setting.Save();
            }
        }

        private static int GetCount()
        {
            var setting = Load();
            return setting.MailsSendedDate.Date < DateTime.UtcNow.Date
                       ? 0
                       : setting.MailsSendedCount;
        }
    }
}