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
using System.Collections.Generic;

using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Returns a list of all the alerts for the authenticated user.
        /// </summary>
        /// <returns type="ASC.Mail.Data.Contracts.MailAlertData, ASC.Mail">List of alerts</returns>
        /// <short>Get alerts</short> 
        /// <category>Alerts</category>
        /// <path>api/2.0/mail/alert</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"alert")]
        public IList<MailAlertData> GetAlerts()
        {
            return MailEngineFactory.AlertEngine.GetAlerts();
        }

        /// <summary>
        /// Deletes an alert with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int64, System" method="url" name="id">Alert ID</param>
        /// <returns>Deleted alert ID. Same as the request parameter</returns>
        /// <short>Delete an alert</short> 
        /// <category>Alerts</category>
        /// <path>api/2.0/mail/alert/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"alert/{id}")]
        public long DeleteAlert(long id)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid alert id. Id must be positive integer.", "id");

            var success = MailEngineFactory.AlertEngine.DeleteAlert(id);

            if (!success)
                throw new Exception("Delete failed");

            return id;
        }
    }
}
