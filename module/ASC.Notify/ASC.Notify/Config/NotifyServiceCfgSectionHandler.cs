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


using System.Configuration;

namespace ASC.Notify.Config
{
    public class NotifyServiceCfgSectionHandler : ConfigurationSection
    {
        [ConfigurationProperty("connectionStringName", IsRequired = true)]
        public string ConnectionStringName
        {
            get { return (string)base["connectionStringName"]; }
        }

        [ConfigurationProperty("storeDays", DefaultValue = 7)]
        [IntegerValidator(MinValue = 1, MaxValue = short.MaxValue)]
        public int StoreMessagesDays
        {
            get { return (int)base["storeDays"]; }
        }

        [ConfigurationProperty("process")]
        public NotifyServiceCfgProcessElement Process
        {
            get { return (NotifyServiceCfgProcessElement)base["process"]; }
        }

        [ConfigurationProperty("senders")]
        [ConfigurationCollection(typeof(NotifyServiceCfgSendersCollection), AddItemName = "sender")]
        public NotifyServiceCfgSendersCollection Senders
        {
            get { return (NotifyServiceCfgSendersCollection)base["senders"]; }
        }

        [ConfigurationProperty("schedulers")]
        [ConfigurationCollection(typeof(NotifyServiceCfgSchedulersCollection), AddItemName = "scheduler")]
        public NotifyServiceCfgSchedulersCollection Schedulers
        {
            get { return (NotifyServiceCfgSchedulersCollection)base["schedulers"]; }
        }
    }
}
