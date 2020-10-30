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


using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Create address for tenant notifications
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="domain_id"></param>
        /// <returns>NotificationAddressData associated with tenant</returns>
        /// <short>Create notification address</short> 
        /// <category>Notifications</category>
        [Create(@"notification/address/add")]
        public ServerNotificationAddressData CreateNotificationAddress(string name, string password, int domain_id)
        {
            var notifyAddress = MailEngineFactory.ServerEngine.CreateNotificationAddress(name, password, domain_id);
            return notifyAddress;
        }

        /// <summary>
        ///    Deletes address for notification 
        /// </summary>
        /// <short>Remove mailbox from mail server</short> 
        /// <category>Notifications</category>
        [Delete(@"notification/address/remove")]
        public void RemoveNotificationAddress(string address)
        {
            MailEngineFactory.ServerEngine.RemoveNotificationAddress(address);
        }
    }
}
