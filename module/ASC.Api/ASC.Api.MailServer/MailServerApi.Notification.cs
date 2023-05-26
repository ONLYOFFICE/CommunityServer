/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

using ASC.Web.Studio.PublicResources;

// ReSharper disable InconsistentNaming

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        /// Creates an address for the tenant notifications with the parameters specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="name">Address name</param>
        /// <param type="System.String, System" name="password">Address password</param>
        /// <param type="System.Int32, System" name="domain_id">Domain ID</param>
        /// <returns type="ASC.Mail.Data.Contracts.ServerNotificationAddressData, ASC.Mail">Notification address data associated with the tenant</returns>
        /// <short>Create a notification address</short> 
        /// <category>Notifications</category>
        /// <path>api/2.0/mailserver/notification/address/add</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"notification/address/add")]
        public ServerNotificationAddressData CreateNotificationAddress(string name, string password, int domain_id)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            var notifyAddress = MailEngineFactory.ServerEngine.CreateNotificationAddress(name, password, domain_id);
            return notifyAddress;
        }

        /// <summary>
        /// Deletes an address for the tenant notifications specified in the request. 
        /// </summary>
        /// <param type="System.String, System" name="address">Address name</param>
        /// <short>Remove a notification address</short> 
        /// <category>Notifications</category>
        /// <path>api/2.0/mailserver/notification/address/remove</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <returns></returns>
        [Delete(@"notification/address/remove")]
        public void RemoveNotificationAddress(string address)
        {
            if (!IsEnableMailServer) throw new Exception(Resource.ErrorNotAllowedOption);
            MailEngineFactory.ServerEngine.RemoveNotificationAddress(address);
        }
    }
}
