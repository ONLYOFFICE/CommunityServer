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


using System.Collections.Generic;
using ASC.Mail.Core.Entities;
using ASC.Mail.Enums;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IAlertDao
    {
        /// <summary>
        ///     Get alert by id.
        /// </summary>
        /// <param name="id">id</param>
        Alert GetAlert(long id);

        /// <summary>
        ///     Get a list of alerts
        /// </summary>
        List<Alert> GetAlerts(int mailboxId = -1, MailAlertTypes type = MailAlertTypes.Empty);

        /// <summary>
        ///     Save or update alert
        /// </summary>
        /// <param name="alert"></param>
        /// <param name="unique"></param>
        int SaveAlert(Alert alert, bool unique = false);

        /// <summary>
        ///     Delete alert
        /// </summary>
        /// <param name="id">id</param>
        int DeleteAlert(long id);

        /// <summary>
        ///     Delete all mailbox's alerts
        /// </summary>
        /// <param name="mailboxId">id mailbox</param>
        int DeleteAlerts(int mailboxId);

        /// <summary>
        ///     Delete alerts
        /// </summary>
        /// <param name="ids">list of id</param>
        int DeleteAlerts(List<int> ids);
    }
}
