/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Security;
using ASC.Api.Attributes;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Mail.Server.Dal;
using ASC.Mail.Server.Utils;

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
        public NotificationAddressData CreateNotificationAddress(string name, string password, int domain_id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", @"Invalid address username.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password", @"Invalid password.");

            if (name.Length > 64)
                throw new ArgumentException(@"Local part of address exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException(@"Incorrect address username.", "name");

            if (!Parser.IsPasswordValid(password))
                throw new ArgumentException(
                    @"Incorrect password. The password's first character must be a letter," +
                    @" it must contain at least 6 characters and no more than 15 characters " +
                    @"and no characters other than letters, numbers and the underscore may be used",
                    "password");

            var localPart = name.ToLowerInvariant();

            if (domain_id < 0)
                throw new ArgumentException(@"Invalid domain id.", "domain_id");

            var domain = MailServer.GetWebDomain(domain_id, MailServerFactory);

            var notificationAddress = MailServer.CreateNotificationAddress(localPart, password, domain, MailServerFactory);

            return notificationAddress.ToNotificationAddressData();

        }

        /// <summary>
        ///    Deletes address for notification 
        /// </summary>
        /// <short>Remove mailbox from mail server</short> 
        /// <category>Notifications</category>
        [Delete(@"notification/address/remove")]
        public void RemoveNotificationAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException(@"Invalid mailbox address.", "address");

            MailServer.DeleteNotificationAddress(address);
        }
    }
}
